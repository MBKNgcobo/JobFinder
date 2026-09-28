using JobFinder.Models;

namespace JobFinder.Services.Interfaces
{
    public interface IJobService
    {
        Task<List<Job>> GetAllJobsAsync(
            string? search = null,
            string? location = null,
            string? source = null,
            int? userId = null,
            decimal? minMatchScore = null);

        Task<Job?> GetJobByIdAsync(int id);

        Task<Job> CreateJobAsync(Job job);

        Task<bool> UpdateJobAsync(Job job);

        Task<bool> DeleteJobAsync(int id);
    }
}