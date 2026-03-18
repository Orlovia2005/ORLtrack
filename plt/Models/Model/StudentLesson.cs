namespace plt.Models.Model
{
    public class StudentLesson
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int TeacherId { get; set; }
        public DateTime LessonDateUtc { get; set; }
        public decimal ChargedAmount { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public Student Student { get; set; } = null!;
        public User Teacher { get; set; } = null!;
    }
}
