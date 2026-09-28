using System.ComponentModel.DataAnnotations;

namespace JobFinder.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Website { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        [MaxLength(100)]
        public string? Industry { get; set; }

        // Relationships
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}