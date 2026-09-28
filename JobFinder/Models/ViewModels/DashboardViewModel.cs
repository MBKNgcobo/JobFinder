using JobFinder.Models.DTOs;

namespace JobFinder.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<JobRecommendationDto> Recommendations { get; set; } = new();

        public List<Application> Applications { get; set; } = new();

        public int TotalApplications { get; set; }

        public int AppliedApplications { get; set; }

        public int DraftApplications { get; set; }
    }
}