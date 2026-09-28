using System.ComponentModel.DataAnnotations;

namespace JobFinder.Models
{
    public class Application
    {
        public int Id { get; set; }

        // Foreign keys
        public int UserId { get; set; }

        public int JobId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public DateTime? AppliedAt { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? CoverLetter { get; set; }

        public string? Notes { get; set; }

        // Relationships
        public User User { get; set; } = null!;

        public Job Job { get; set; } = null!;

        public ICollection<ApplicationSkill> ApplicationSkills { get; set; } =
            new List<ApplicationSkill>();

        //cv
        public string? GeneratedCoverLetter { get; set; }

        public string? GeneratedCvChanges { get; set; }

        public string? GeneratedMatchedSkills { get; set; }

        public string? GeneratedProjects { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public ICollection<ApplicationDocument> Documents { get; set; } = new List<ApplicationDocument>();
    }

  

}