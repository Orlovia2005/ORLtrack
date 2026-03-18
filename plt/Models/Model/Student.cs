using System.ComponentModel.DataAnnotations;

namespace plt.Models.Model
{
    public class Student
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public decimal Balance { get; set; }
        public decimal LessonRate { get; set; }
        public int LessonsAttendedCount { get; set; }
        public decimal TotalPaidIn { get; set; }
        public decimal TotalCharged { get; set; }
        public DateTime? LastLessonAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public User Teacher { get; set; } = null!;
        public ICollection<StudentLesson> Lessons { get; set; } = new List<StudentLesson>();
        public ICollection<StudentPayment> Payments { get; set; } = new List<StudentPayment>();
    }
}
