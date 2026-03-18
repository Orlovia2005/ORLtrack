using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using plt.Models.ViewModel;

namespace plt.Services.Ai
{
    public interface IStudentRiskAiService
    {
        Task<IReadOnlyList<AiStudentInsightViewModel>> BuildInsightsAsync(IEnumerable<StudentDashboardItemViewModel> students, CancellationToken cancellationToken = default);
        bool IsConfigured { get; }
        string ProviderName { get; }
    }

    public class StudentRiskAiService : IStudentRiskAiService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;
        private readonly GigaChatOptions _options;
        private readonly ILogger<StudentRiskAiService> _logger;

        private string? _accessToken;
        private DateTimeOffset _tokenExpiresAtUtc;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);

        public StudentRiskAiService(HttpClient httpClient, IOptions<GigaChatOptions> options, ILogger<StudentRiskAiService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public bool IsConfigured => _options.Enabled && !string.IsNullOrWhiteSpace(_options.AuthorizationKey);
        public string ProviderName => "GigaChat";

        public async Task<IReadOnlyList<AiStudentInsightViewModel>> BuildInsightsAsync(IEnumerable<StudentDashboardItemViewModel> students, CancellationToken cancellationToken = default)
        {
            var targets = students
                .OrderByDescending(x => x.ChurnRiskScore)
                .Take(Math.Max(1, _options.MaxStudentsPerRequest))
                .ToList();

            if (!targets.Any())
            {
                return Array.Empty<AiStudentInsightViewModel>();
            }

            if (!IsConfigured)
            {
                return targets.Select(BuildLocalInsight).ToList();
            }

            var insights = new List<AiStudentInsightViewModel>();
            foreach (var student in targets)
            {
                try
                {
                    insights.Add(await BuildGigaChatInsightAsync(student, cancellationToken));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось получить AI-анализ для ученика {StudentId}. Использую локальный fallback.", student.Id);
                    insights.Add(BuildLocalInsight(student));
                }
            }

            return insights;
        }

        private async Task<AiStudentInsightViewModel> BuildGigaChatInsightAsync(StudentDashboardItemViewModel student, CancellationToken cancellationToken)
        {
            var token = await GetAccessTokenAsync(cancellationToken);

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.ChatUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var systemPrompt = """
                Ты аналитик образовательной платформы ORLtrack.
                Твоя задача: по признакам ученика оценить риск ухода и объяснить его преподавателю простым русским языком.
                Отвечай строго в JSON с полями:
                summary: короткое объяснение на 1-2 предложения,
                recommendation: одно конкретное следующее действие преподавателя,
                tone: одно слово из набора low, medium, high.
                Не используй markdown.
                """;

            var userPrompt = $$"""
                Проанализируй ученика и объясни риск ухода.

                Имя: {{student.FullName}}
                Риск: {{student.ChurnRiskScore}} из 100
                Уровень риска: {{student.ChurnRiskLevel}}
                Дней с последнего занятия: {{student.DaysSinceLastLesson}}
                Дней с последнего пополнения: {{student.DaysSinceLastPayment}}
                Пропусков за последние уроки: {{student.RecentSkipsCount}}
                Оплаченных занятий в недавней истории: {{student.RecentPaidLessonsCount}}
                Баланс: {{student.Balance}}
                Ставка: {{student.LessonRate}}
                Всего занятий: {{student.LessonsAttendedCount}}
                Признаки риска: {{string.Join("; ", student.RiskSignals)}}
                """;

            var body = new
            {
                model = _options.Model,
                temperature = 0.3,
                max_tokens = 500,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            var parsed = JsonSerializer.Deserialize<GigaChatCompletionResponse>(payload, JsonOptions);
            var content = parsed?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                return BuildLocalInsight(student);
            }

            try
            {
                var ai = JsonSerializer.Deserialize<GigaChatInsightPayload>(content, JsonOptions);
                if (ai == null || string.IsNullOrWhiteSpace(ai.Summary))
                {
                    return BuildLocalInsight(student);
                }

                return new AiStudentInsightViewModel
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    RiskScore = student.ChurnRiskScore,
                    RiskLevel = student.ChurnRiskLevel,
                    Summary = ai.Summary.Trim(),
                    Recommendation = string.IsNullOrWhiteSpace(ai.Recommendation) ? BuildFallbackRecommendation(student) : ai.Recommendation.Trim(),
                    Provider = ProviderName
                };
            }
            catch
            {
                return new AiStudentInsightViewModel
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    RiskScore = student.ChurnRiskScore,
                    RiskLevel = student.ChurnRiskLevel,
                    Summary = content,
                    Recommendation = BuildFallbackRecommendation(student),
                    Provider = ProviderName
                };
            }
        }

        private AiStudentInsightViewModel BuildLocalInsight(StudentDashboardItemViewModel student)
        {
            var summary = student.ChurnRiskLevel switch
            {
                "Высокий" => $"{student.FullName} выглядит учеником с высоким риском ухода: активность просела, а текущие сигналы показывают потерю вовлечённости.",
                "Средний" => $"{student.FullName} пока не ушёл в красную зону, но поведение стало менее стабильным и требует внимания преподавателя.",
                _ => $"{student.FullName} сейчас остаётся стабильным учеником, но ORLtrack продолжает отслеживать изменения в посещаемости и оплате."
            };

            return new AiStudentInsightViewModel
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                RiskScore = student.ChurnRiskScore,
                RiskLevel = student.ChurnRiskLevel,
                Summary = summary,
                Recommendation = BuildFallbackRecommendation(student),
                Provider = "Локальный AI fallback"
            };
        }

        private static string BuildFallbackRecommendation(StudentDashboardItemViewModel student)
        {
            if (student.Balance < student.LessonRate && student.LessonRate > 0)
            {
                return "Свяжитесь с учеником и мягко напомните о пополнении баланса перед следующим занятием.";
            }

            if (student.RecentSkipsCount > 0)
            {
                return "Уточните причину последних пропусков и предложите более удобное время занятий.";
            }

            if (student.DaysSinceLastLesson >= 14)
            {
                return "Напишите ученику в ближайшие дни и предложите зафиксировать следующее занятие в расписании.";
            }

            return "Поддерживайте регулярный контакт и заранее подтверждайте следующее занятие.";
        }

        private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_accessToken) && _tokenExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(2))
            {
                return _accessToken;
            }

            await _tokenLock.WaitAsync(cancellationToken);
            try
            {
                if (!string.IsNullOrWhiteSpace(_accessToken) && _tokenExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(2))
                {
                    return _accessToken;
                }

                using var request = new HttpRequestMessage(HttpMethod.Post, _options.OAuthUrl);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("RqUID", Guid.NewGuid().ToString());
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _options.AuthorizationKey);
                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["scope"] = _options.Scope
                });

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var payload = await response.Content.ReadAsStringAsync(cancellationToken);
                response.EnsureSuccessStatusCode();

                var tokenResponse = JsonSerializer.Deserialize<GigaChatTokenResponse>(payload, JsonOptions)
                    ?? throw new InvalidOperationException("GigaChat token response is empty.");

                _accessToken = tokenResponse.AccessToken;
                _tokenExpiresAtUtc = DateTimeOffset.FromUnixTimeMilliseconds(tokenResponse.ExpiresAt);
                return _accessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private sealed class GigaChatTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonPropertyName("expires_at")]
            public long ExpiresAt { get; set; }
        }

        private sealed class GigaChatCompletionResponse
        {
            public List<GigaChatChoice>? Choices { get; set; }
        }

        private sealed class GigaChatChoice
        {
            public GigaChatMessage? Message { get; set; }
        }

        private sealed class GigaChatMessage
        {
            public string? Content { get; set; }
        }

        private sealed class GigaChatInsightPayload
        {
            public string? Summary { get; set; }
            public string? Recommendation { get; set; }
            public string? Tone { get; set; }
        }
    }
}
