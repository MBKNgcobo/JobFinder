using JobFinder.Models.DTOs;

namespace JobFinder.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<JobRecommendationDto>> GetRecommendationsAsync(
            int userId,
            string? location = null);
    }
}