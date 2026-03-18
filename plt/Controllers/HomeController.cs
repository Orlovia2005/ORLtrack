using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using plt.Models.Model;
using plt.Models.ViewModel;
using plt.Services.Ai;

namespace plt.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStudentRiskAiService _studentRiskAiService;

        public HomeController(
            EducationDbContext context,
            ILogger<HomeController> logger,
            IStudentRiskAiService studentRiskAiService) : base(context)
        {
            _logger = logger;
            _studentRiskAiService = studentRiskAiService;
        }

        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == null)
            {
                return View(new DashboardViewModel());
            }

            var model = await BuildDashboardAsync(CurrentUserId.Value);
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Students()
        {
            var model = await BuildDashboardAsync(CurrentUserId!.Value);
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Analytics()
        {
            var model = await BuildDashboardAsync(CurrentUserId!.Value);
            model.AiProviderName = _studentRiskAiService.ProviderName;
            model.IsRussianAiConfigured = _studentRiskAiService.IsConfigured;
            model.AiInsights = (await _studentRiskAiService.BuildInsightsAsync(model.Students)).ToList();
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(string firstName, string? lastName, string? email, string? phone, decimal lessonRate, decimal? startingBalance)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                Notif_Error("Введите имя ученика.");
                return RedirectToAction(nameof(Students));
            }

            if (lessonRate <= 0)
            {
                Notif_Error("Ставка за занятие должна быть больше нуля.");
                return RedirectToAction(nameof(Students));
            }

            var student = new Student
            {
                TeacherId = CurrentUserId.Value,
                FirstName = firstName.Trim(),
                LastName = (lastName ?? string.Empty).Trim(),
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
                LessonRate = lessonRate,
                Balance = Math.Max(0, startingBalance ?? 0),
                TotalPaidIn = Math.Max(0, startingBalance ?? 0),
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Students.Add(student);

            if ((startingBalance ?? 0) > 0)
            {
                _context.StudentPayments.Add(new StudentPayment
                {
                    Student = student,
                    TeacherId = CurrentUserId.Value,
                    Amount = startingBalance!.Value,
                    PaymentDateUtc = DateTime.UtcNow,
                    Comment = "Стартовое пополнение",
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            Notif_Success("Ученик добавлен.");
            return RedirectToAction(nameof(Students));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBalance(int studentId, decimal amount, string? comment)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (amount <= 0)
            {
                Notif_Error("Сумма пополнения должна быть больше нуля.");
                return RedirectToAction(nameof(Students));
            }

            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId && x.TeacherId == CurrentUserId.Value);
            if (student == null)
            {
                Notif_Error("Ученик не найден.");
                return RedirectToAction(nameof(Students));
            }

            student.Balance += amount;
            student.TotalPaidIn += amount;

            _context.StudentPayments.Add(new StudentPayment
            {
                StudentId = student.Id,
                TeacherId = CurrentUserId.Value,
                Amount = amount,
                PaymentDateUtc = DateTime.UtcNow,
                Comment = string.IsNullOrWhiteSpace(comment) ? "Пополнение баланса" : comment.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            Notif_Success($"Баланс ученика {student.FirstName} пополнен.");
            return RedirectToAction(nameof(Students));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkLesson(int studentId, DateTime? lessonDate, string? comment)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId && x.TeacherId == CurrentUserId.Value);
            if (student == null)
            {
                Notif_Error("Ученик не найден.");
                return RedirectToAction(nameof(Students));
            }

            if (student.LessonRate <= 0)
            {
                Notif_Error("Для ученика не задана корректная ставка за занятие.");
                return RedirectToAction(nameof(Students));
            }

            if (student.Balance < student.LessonRate)
            {
                Notif_Error($"Недостаточно средств на балансе. Нужно минимум {student.LessonRate:N2}.");
                return RedirectToAction(nameof(Students));
            }

            var actualLessonDate = lessonDate?.ToUniversalTime() ?? DateTime.UtcNow;

            student.Balance -= student.LessonRate;
            student.LessonsAttendedCount += 1;
            student.TotalCharged += student.LessonRate;
            student.LastLessonAtUtc = actualLessonDate;

            _context.StudentLessons.Add(new StudentLesson
            {
                StudentId = student.Id,
                TeacherId = CurrentUserId.Value,
                LessonDateUtc = actualLessonDate,
                ChargedAmount = student.LessonRate,
                Comment = string.IsNullOrWhiteSpace(comment) ? "Проведено занятие" : comment.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            Notif_Success($"Занятие для {student.FirstName} отмечено, баланс списан.");
            return RedirectToAction(nameof(Students));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SkipLesson(int studentId, DateTime? lessonDate, string? comment)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId && x.TeacherId == CurrentUserId.Value);
            if (student == null)
            {
                Notif_Error("Ученик не найден.");
                return RedirectToAction(nameof(Students));
            }

            var actualLessonDate = lessonDate?.ToUniversalTime() ?? DateTime.UtcNow;

            _context.StudentLessons.Add(new StudentLesson
            {
                StudentId = student.Id,
                TeacherId = CurrentUserId.Value,
                LessonDateUtc = actualLessonDate,
                ChargedAmount = 0,
                Comment = string.IsNullOrWhiteSpace(comment) ? "Пропуск занятия" : comment.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            Notif_Info($"Для {student.FirstName} записан пропуск без списания баланса.");
            return RedirectToAction(nameof(Students));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRate(int studentId, decimal lessonRate)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (lessonRate <= 0)
            {
                Notif_Error("Новая ставка должна быть больше нуля.");
                return RedirectToAction(nameof(Students));
            }

            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId && x.TeacherId == CurrentUserId.Value);
            if (student == null)
            {
                Notif_Error("Ученик не найден.");
                return RedirectToAction(nameof(Students));
            }

            student.LessonRate = lessonRate;
            await _context.SaveChangesAsync();

            Notif_Success($"Ставка для {student.FirstName} обновлена.");
            return RedirectToAction(nameof(Students));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<DashboardViewModel> BuildDashboardAsync(int userId)
        {
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var students = await _context.Students
                .AsNoTracking()
                .Where(x => x.TeacherId == userId)
                .Include(x => x.Lessons.OrderByDescending(l => l.LessonDateUtc).Take(6))
                .Include(x => x.Payments.OrderByDescending(p => p.PaymentDateUtc).Take(6))
                .OrderByDescending(x => x.LastLessonAtUtc)
                .ThenBy(x => x.FirstName)
                .ToListAsync();

            var monthLessons = students
                .SelectMany(x => x.Lessons)
                .Where(x => x.LessonDateUtc >= monthStart && x.ChargedAmount > 0)
                .ToList();

            var mappedStudents = students.Select(MapStudentToDashboard).ToList();

            var activities = students
                .SelectMany(student =>
                    student.Payments.Select(payment => new ActivityFeedItemViewModel
                    {
                        StudentName = BuildFullName(student.FirstName, student.LastName),
                        Title = "Пополнение баланса",
                        Subtitle = string.IsNullOrWhiteSpace(payment.Comment) ? "Баланс пополнен" : payment.Comment!,
                        EventDateUtc = payment.PaymentDateUtc,
                        IsIncome = true,
                        Amount = payment.Amount
                    })
                    .Concat(student.Lessons.Select(lesson => new ActivityFeedItemViewModel
                    {
                        StudentName = BuildFullName(student.FirstName, student.LastName),
                        Title = lesson.ChargedAmount > 0 ? "Проведено занятие" : "Зафиксирован пропуск",
                        Subtitle = string.IsNullOrWhiteSpace(lesson.Comment)
                            ? lesson.ChargedAmount > 0 ? "Оплата списана автоматически" : "Баланс не изменялся"
                            : lesson.Comment!,
                        EventDateUtc = lesson.LessonDateUtc,
                        IsIncome = false,
                        Amount = lesson.ChargedAmount,
                        IsSkipped = lesson.ChargedAmount <= 0
                    })))
                .OrderByDescending(x => x.EventDateUtc)
                .Take(10)
                .ToList();

            return new DashboardViewModel
            {
                IsAuthenticated = true,
                UserName = User.Identity?.Name ?? "преподаватель",
                StudentsCount = mappedStudents.Count,
                TotalBalance = mappedStudents.Sum(x => x.Balance),
                TotalLessons = mappedStudents.Sum(x => x.LessonsAttendedCount),
                TotalRevenue = mappedStudents.Sum(x => x.TotalCharged),
                MonthlyRevenue = monthLessons.Sum(x => x.ChargedAmount),
                MonthlyLessons = monthLessons.Count,
                AverageLessonRate = mappedStudents.Any() ? mappedStudents.Average(x => x.LessonRate) : 0,
                StudentsWithLowBalance = mappedStudents.Count(x => x.Balance < x.LessonRate && x.LessonRate > 0),
                Students = mappedStudents.OrderByDescending(x => x.ChurnRiskScore).ThenBy(x => x.FullName).ToList(),
                TopStudents = mappedStudents
                    .OrderByDescending(x => x.TotalCharged)
                    .ThenByDescending(x => x.LessonsAttendedCount)
                    .Take(4)
                    .ToList(),
                LowBalanceStudents = mappedStudents
                    .Where(x => x.Balance < x.LessonRate && x.LessonRate > 0)
                    .OrderBy(x => x.Balance)
                    .Take(5)
                    .ToList(),
                RecentActivities = activities
            };
        }

        private static StudentDashboardItemViewModel MapStudentToDashboard(Student student)
        {
            var fullName = BuildFullName(student.FirstName, student.LastName);
            var recentLessons = student.Lessons.OrderByDescending(x => x.LessonDateUtc).Take(3).ToList();
            var recentPayments = student.Payments.OrderByDescending(x => x.PaymentDateUtc).Take(3).ToList();

            var lastPayment = student.Payments.OrderByDescending(x => x.PaymentDateUtc).FirstOrDefault();
            var daysSinceLastLesson = student.LastLessonAtUtc.HasValue
                ? (int)Math.Max(0, (DateTime.UtcNow - student.LastLessonAtUtc.Value).TotalDays)
                : 999;
            var daysSinceLastPayment = lastPayment != null
                ? (int)Math.Max(0, (DateTime.UtcNow - lastPayment.PaymentDateUtc).TotalDays)
                : 999;

            var recentSkipsCount = student.Lessons.Count(x => x.ChargedAmount <= 0);
            var recentPaidLessonsCount = student.Lessons.Count(x => x.ChargedAmount > 0);

            var riskSignals = BuildRiskSignals(student, daysSinceLastLesson, daysSinceLastPayment, recentSkipsCount);
            var riskScore = CalculateRiskScore(student, daysSinceLastLesson, daysSinceLastPayment, recentSkipsCount);

            return new StudentDashboardItemViewModel
            {
                Id = student.Id,
                FullName = fullName,
                Initials = BuildInitials(student.FirstName, student.LastName),
                Email = student.Email,
                Phone = student.Phone,
                Balance = student.Balance,
                LessonRate = student.LessonRate,
                LessonsAttendedCount = student.LessonsAttendedCount,
                TotalPaidIn = student.TotalPaidIn,
                TotalCharged = student.TotalCharged,
                LastLessonAtUtc = student.LastLessonAtUtc,
                DaysSinceLastLesson = daysSinceLastLesson,
                DaysSinceLastPayment = daysSinceLastPayment,
                RecentSkipsCount = recentSkipsCount,
                RecentPaidLessonsCount = recentPaidLessonsCount,
                ChurnRiskScore = riskScore,
                ChurnRiskLevel = riskScore >= 70 ? "Высокий" : riskScore >= 40 ? "Средний" : "Низкий",
                RiskSignals = riskSignals,
                RecentPayments = recentPayments.Select(p => new StudentPaymentHistoryItemViewModel
                {
                    Amount = p.Amount,
                    PaymentDateUtc = p.PaymentDateUtc,
                    Comment = p.Comment
                }).ToList(),
                RecentLessons = recentLessons.Select(l => new StudentLessonHistoryItemViewModel
                {
                    ChargedAmount = l.ChargedAmount,
                    LessonDateUtc = l.LessonDateUtc,
                    Comment = l.Comment,
                    IsSkipped = l.ChargedAmount <= 0
                }).ToList()
            };
        }

        private static int CalculateRiskScore(Student student, int daysSinceLastLesson, int daysSinceLastPayment, int recentSkipsCount)
        {
            var score = 5;

            if (daysSinceLastLesson >= 30) score += 35;
            else if (daysSinceLastLesson >= 14) score += 22;
            else if (daysSinceLastLesson >= 7) score += 10;

            if (daysSinceLastPayment >= 30) score += 18;
            else if (daysSinceLastPayment >= 14) score += 10;

            if (student.Balance < student.LessonRate && student.LessonRate > 0) score += 18;
            if (recentSkipsCount >= 2) score += 20;
            else if (recentSkipsCount == 1) score += 10;

            if (student.LessonsAttendedCount == 0) score += 12;

            return Math.Min(100, score);
        }

        private static List<string> BuildRiskSignals(Student student, int daysSinceLastLesson, int daysSinceLastPayment, int recentSkipsCount)
        {
            var signals = new List<string>();

            if (daysSinceLastLesson >= 14)
            {
                signals.Add($"давно не было занятий: {daysSinceLastLesson} дн.");
            }

            if (daysSinceLastPayment >= 14)
            {
                signals.Add($"давно не было пополнений: {daysSinceLastPayment} дн.");
            }

            if (student.Balance < student.LessonRate && student.LessonRate > 0)
            {
                signals.Add("баланс ниже стоимости следующего урока");
            }

            if (recentSkipsCount > 0)
            {
                signals.Add($"пропусков в недавней истории: {recentSkipsCount}");
            }

            if (!signals.Any())
            {
                signals.Add("стабильная активность без явных тревожных признаков");
            }

            return signals;
        }

        private static string BuildFullName(string firstName, string lastName)
        {
            return string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static string BuildInitials(string firstName, string lastName)
        {
            var letters = $"{firstName.FirstOrDefault()}{lastName.FirstOrDefault()}".Trim();
            return string.IsNullOrWhiteSpace(letters) ? "OR" : letters.ToUpperInvariant();
        }
    }
}
