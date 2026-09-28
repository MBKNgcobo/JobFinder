using JobFinder.Models.DTOs;

namespace JobFinder.Services.Interfaces
{
    public interface IApplicationAgentService
    {
        Task<ApplicationAgentResponseDto?> GenerateApplicationAsync(
            ApplicationAgentRequestDto request);
    }
}