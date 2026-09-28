using JobFinder.Models;
using JobFinder.Models.DTOs;

namespace JobFinder.Models.ViewModels
{
    public class ApplicationDetailsViewModel
    {
        public int Id { get; set; }

        public int JobId { get; set; }

        public string Status { get; set; } = "Draft";

        public DateTime? AppliedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // ============================================================
        // GENERATED APPLICATION DATA
        // ============================================================

        public string? GeneratedCoverLetter { get; set; }

        public string? GeneratedCvChanges { get; set; }

        public string? GeneratedMatchedSkills { get; set; }

        public string? GeneratedProjects { get; set; }

        public DateTime? GeneratedAt { get; set; }

        // ============================================================
        // TYPED GENERATED DATA
        // ============================================================

        public List<string> MatchedSkills { get; set; }
            = new();

        public List<string> CvChanges { get; set; }
            = new();

        public List<RecommendedProjectDto> RecommendedProjects { get; set; }
            = new();

        // ============================================================
        // JOB
        // ============================================================

        public Job? Job { get; set; }

        // ============================================================
        // GENERATED DOCUMENTS
        // ============================================================

        public List<ApplicationDocumentViewModel> Documents { get; set; }
            = new();
    }


    public class ApplicationDocumentViewModel
    {
        public int Id { get; set; }

        public string DocumentType { get; set; } = string.Empty;

        public int Version { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}