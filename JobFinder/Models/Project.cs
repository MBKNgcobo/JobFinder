using System.ComponentModel.DataAnnotations;

namespace JobFinder.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? GitHubUrl { get; set; }

        public string? LiveUrl { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // Foreign key
        public int UserId { get; set; }

        // Relationships
        public User User { get; set; } = null!;

        public ICollection<ProjectSkill> ProjectSkills { get; set; } =
            new List<ProjectSkill>();
        
    }
}