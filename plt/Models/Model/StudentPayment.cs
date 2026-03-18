namespace plt.Models.Model
{
    public class StudentPayment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int TeacherId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDateUtc { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public Student Student { get; set; } = null!;
        public User Teacher { get; set; } = null!;
    }
}
