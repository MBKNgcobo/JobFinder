namespace JobFinder.Models.DTOs
{
    public class ApplicationDetailsDto
    {
        public int Id { get; set; }

        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;

        public string? Location { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? AppliedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}