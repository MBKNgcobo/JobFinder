namespace JobFinder.Models
{
    public class JobMatch
    {
        public int Id { get; set; }

        // Foreign keys
        public int UserId { get; set; }

        public int JobId { get; set; }

        public decimal MatchScore { get; set; }

        public string? Analysis { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public User User { get; set; } = null!;

        public Job Job { get; set; } = null!;
    }
}