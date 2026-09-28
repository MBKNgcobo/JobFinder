using System.ComponentModel.DataAnnotations;

namespace JobFinder.Models
{
    public class Skill
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        // Relationships
        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();

        public ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();

        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();

        public ICollection<ApplicationSkill> ApplicationSkills { get; set; } =
            new List<ApplicationSkill>();
    }
}