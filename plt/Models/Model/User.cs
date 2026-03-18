namespace plt.Models.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<StudentLesson> StudentLessons { get; set; } = new List<StudentLesson>();
        public ICollection<StudentPayment> StudentPayments { get; set; } = new List<StudentPayment>();
    }
}
