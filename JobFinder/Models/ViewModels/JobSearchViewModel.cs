using JobFinder.Models;

namespace JobFinder.Models.ViewModels
{
    public class JobSearchViewModel
    {
        public string? Search { get; set; }

        public string? Location { get; set; }

        public string? Source { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalJobs { get; set; }

        public int TotalPages =>
            PageSize <= 0
                ? 1
                : (int)Math.Ceiling(
                    TotalJobs / (double)PageSize);

        public List<Job> Jobs { get; set; } = new();
    }
}