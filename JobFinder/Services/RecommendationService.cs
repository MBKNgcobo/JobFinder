using System.Net.Http.Json;
using JobFinder.Models.DTOs;
using JobFinder.Services.Interfaces;

namespace JobFinder.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly HttpClient _httpClient;

        public RecommendationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<JobRecommendationDto>>
            GetRecommendationsAsync(
                int userId,
                string? location = null)
        {
            var url =
                $"api/agents/recommendations/{userId}";

            if (!string.IsNullOrWhiteSpace(location))
            {
                url +=
                    $"?preferred_location={Uri.EscapeDataString(location)}";
            }

            var response =
                await _httpClient.GetFromJsonAsync<
                    RecommendationResponseDto
                >(url);

            return response?.Recommendations
                ?? new List<JobRecommendationDto>();
        }
    }
}