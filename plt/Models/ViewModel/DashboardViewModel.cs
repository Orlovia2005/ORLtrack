namespace plt.Models.ViewModel
{
    public class DashboardViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime AbsencePeriodStartUtc { get; set; }
        public DateTime AbsencePeriodEndUtc { get; set; }
        public int StudentsCount { get; set; }
        public decimal TotalBalance { get; set; }
        public int TotalLessons { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int MonthlyLessons { get; set; }
        public decimal AverageLessonRate { get; set; }
        public int StudentsWithLowBalance { get; set; }
        public bool IsRussianAiConfigured { get; set; }
        public string AiProviderName { get; set; } = string.Empty;
        public List<StudentDashboardItemViewModel> Students { get; set; } = new();
        public List<StudentDashboardItemViewModel> TopStudents { get; set; } = new();
        public List<StudentDashboardItemViewModel> LowBalanceStudents { get; set; } = new();
        public List<ActivityFeedItemViewModel> RecentActivities { get; set; } = new();
        public List<AiStudentInsightViewModel> AiInsights { get; set; } = new();
    }

    public class StudentDashboardItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public decimal Balance { get; set; }
        public decimal LessonRate { get; set; }
        public int LessonsAttendedCount { get; set; }
        public decimal TotalPaidIn { get; set; }
        public decimal TotalCharged { get; set; }
        public DateTime? LastLessonAtUtc { get; set; }
        public int DaysSinceLastLesson { get; set; }
        public int DaysSinceLastPayment { get; set; }
        public int RecentSkipsCount { get; set; }
        public int RecentPaidLessonsCount { get; set; }
        public int ChurnRiskScore { get; set; }
        public string ChurnRiskLevel { get; set; } = "Низкий";
        public int FilteredMissedLessonsCount { get; set; }
        public List<string> RiskSignals { get; set; } = new();
        public List<StudentPaymentHistoryItemViewModel> RecentPayments { get; set; } = new();
        public List<StudentLessonHistoryItemViewModel> RecentLessons { get; set; } = new();
        public List<StudentLessonHistoryItemViewModel> FilteredMissedLessons { get; set; } = new();
    }

    public class StudentPaymentHistoryItemViewModel
    {
        public decimal Amount { get; set; }
        public DateTime PaymentDateUtc { get; set; }
        public string? Comment { get; set; }
    }

    public class StudentLessonHistoryItemViewModel
    {
        public int Id { get; set; }
        public decimal ChargedAmount { get; set; }
        public DateTime LessonDateUtc { get; set; }
        public string? Comment { get; set; }
        public bool IsSkipped { get; set; }
        public bool IsMadeUp { get; set; }
    }

    public class ActivityFeedItemViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public DateTime EventDateUtc { get; set; }
        public bool IsIncome { get; set; }
        public decimal Amount { get; set; }
        public bool IsSkipped { get; set; }
    }

    public class AiStudentInsightViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
    }
}
