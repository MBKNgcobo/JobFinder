using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace JobFinder.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Location { get; set; }

        [MaxLength(50)]
        public string? EmploymentType { get; set; }

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public DateTime? PostedDate { get; set; }

        public DateTime? ClosingDate { get; set; }

        [MaxLength(100)]
        public string? Source { get; set; }

        public string? SourceUrl { get; set; }

        // Foreign key
        public int CompanyId { get; set; }

        // Relationships
        public Company Company { get; set; } = null!;

        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();

        public ICollection<Application> Applications { get; set; } =
            new List<Application>();

        public ICollection<JobMatch> JobMatches { get; set; } =
            new List<JobMatch>();
    }
}