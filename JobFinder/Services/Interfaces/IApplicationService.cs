using JobFinder.Models;

namespace JobFinder.Services.Interfaces
{
    public interface IApplicationService
    {
        Task<List<Application>> GetUserApplicationsAsync(
            int userId);

        Task<Application?> GetApplicationByIdAsync(
            int id);

        Task<Application?> CreateApplicationAsync(
            int userId,
            int jobId);

        Task<bool> UpdateStatusAsync(
            int applicationId,
            string status);

        Task<bool> UpdateApplicationAsync(Application application);
    }
}