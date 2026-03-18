namespace plt.Models.ViewModel
{
    public class DashboardViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int StudentsCount { get; set; }
        public decimal TotalBalance { get; set; }
        public int TotalLessons { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int MonthlyLessons { get; set; }
        public decimal AverageLessonRate { get; set; }
        public int StudentsWithLowBalance { get; set; }
        public List<StudentDashboardItemViewModel> Students { get; set; } = new();
        public List<StudentDashboardItemViewModel> TopStudents { get; set; } = new();
        public List<StudentDashboardItemViewModel> LowBalanceStudents { get; set; } = new();
        public List<ActivityFeedItemViewModel> RecentActivities { get; set; } = new();
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
        public List<StudentPaymentHistoryItemViewModel> RecentPayments { get; set; } = new();
        public List<StudentLessonHistoryItemViewModel> RecentLessons { get; set; } = new();
    }

    public class StudentPaymentHistoryItemViewModel
    {
        public decimal Amount { get; set; }
        public DateTime PaymentDateUtc { get; set; }
        public string? Comment { get; set; }
    }

    public class StudentLessonHistoryItemViewModel
    {
        public decimal ChargedAmount { get; set; }
        public DateTime LessonDateUtc { get; set; }
        public string? Comment { get; set; }
        public bool IsSkipped { get; set; }
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
}
