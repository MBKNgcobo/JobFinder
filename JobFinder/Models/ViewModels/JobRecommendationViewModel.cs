
namespace JobFinder.Models.ViewModels
{
    public class JobRecommendationViewModel
    {
        public int JobId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? CompanyName { get; set; }

        public string? Location { get; set; }

        public string? EmploymentType { get; set; }

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public string? Source { get; set; }

        public string? SourceUrl { get; set; }

        public decimal MatchScore { get; set; }

        public List<string> MatchedRequiredSkills { get; set; }
            = new();

        public List<string> MissingRequiredSkills { get; set; }
            = new();

        public List<string> MatchedPreferredSkills { get; set; }
            = new();

        public List<string> MissingPreferredSkills { get; set; }
            = new();

        public DateTime? PostedDate { get; set; }
    }
}

