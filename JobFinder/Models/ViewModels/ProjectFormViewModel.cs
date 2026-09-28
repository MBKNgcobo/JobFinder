using System.ComponentModel.DataAnnotations;

namespace JobFinder.Models.ViewModels
{
    public class ProjectFormViewModel
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

        public int UserId { get; set; }

        public List<int> SelectedSkillIds { get; set; } = new();

        public List<Skill> AvailableSkills { get; set; } = new();
    }
}