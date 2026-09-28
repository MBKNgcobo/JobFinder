using System.ComponentModel.DataAnnotations;

namespace JobFinder.Models
{
    public class ApplicationDocument
    {
        public int Id { get; set; }

        public int ApplicationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DocumentType { get; set; } = string.Empty;

        public int Version { get; set; } = 1;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Application Application { get; set; } = null!;
    }
}