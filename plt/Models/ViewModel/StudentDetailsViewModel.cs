namespace plt.Models.ViewModel
{
    public class StudentDetailsViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public DateTime AbsencePeriodStartUtc { get; set; }
        public DateTime AbsencePeriodEndUtc { get; set; }
        public StudentDashboardItemViewModel Student { get; set; } = new();
    }
}
