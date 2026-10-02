using System.ComponentModel.DataAnnotations;

namespace JobFinder.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Grants administrative access to shared reference data
        /// (companies and skills) and to every user-owned record.
        /// </summary>
        public bool IsAdmin { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        public string? Summary { get; set; }

        public int YearsOfExperience { get; set; }

        public string? LinkedInUrl { get; set; }

        public string? GitHubUrl { get; set; }

        public string? PortfolioUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserSkill> UserSkills { get; set; } =
            new List<UserSkill>();

        public ICollection<Project> Projects { get; set; } =
            new List<Project>();

        public ICollection<Application> Applications { get; set; } =
            new List<Application>();

        public ICollection<JobMatch> JobMatches { get; set; } =
            new List<JobMatch>();
    }
}