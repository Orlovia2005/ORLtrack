using plt.Models.ViewModel;
using plt.Services.Ai;

namespace ORLtrack.Tests.Infrastructure;

internal sealed class TestStudentRiskAiService : IStudentRiskAiService
{
    public Task<IReadOnlyList<AiStudentInsightViewModel>> BuildInsightsAsync(IEnumerable<StudentDashboardItemViewModel> students, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<AiStudentInsightViewModel>>(Array.Empty<AiStudentInsightViewModel>());
    }

    public bool IsConfigured => false;
    public string ProviderName => "Test";
}
