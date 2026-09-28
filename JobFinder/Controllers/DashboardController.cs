using JobFinder.Models.DTOs;
using JobFinder.Models.ViewModels;
using JobFinder.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace JobFinder.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IRecommendationService _recommendationService;
        private readonly IApplicationService _applicationService;
        private readonly ICurrentUserService _currentUserService;

        public DashboardController(
            IRecommendationService recommendationService,
            IApplicationService applicationService,
            ICurrentUserService currentUserService)
        {
            _recommendationService = recommendationService;
            _applicationService = applicationService;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index()
        {
            int userId = _currentUserService.UserId;

            var recommendations =
                await _recommendationService
                    .GetRecommendationsAsync(userId);

            var applications =
                await _applicationService
                    .GetUserApplicationsAsync(userId);

            var model = new DashboardViewModel
            {
                Recommendations =
                    recommendations ?? new List<JobRecommendationDto>(),

                Applications = applications.ToList(),

                TotalApplications = applications.Count(),

                AppliedApplications =
                    applications.Count(a => a.Status == "Applied"),

                DraftApplications =
                    applications.Count(a => a.Status == "Draft")
            };

            return View(model);
        }
    }
}