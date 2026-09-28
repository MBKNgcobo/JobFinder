using JobFinder.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobFinder.Controllers
{
    public class RecommendationsController : Controller
    {
        private readonly IRecommendationService
            _recommendationService;
        private readonly ICurrentUserService _currentUserService;

        public RecommendationsController(
            IRecommendationService recommendationService, ICurrentUserService currentUserService)
        {
            _recommendationService =
                recommendationService;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index(
            string? location = null)
        {
            // Temporary user ID.
            // We will replace this with the
            // authenticated user's ID later.
            int userId = _currentUserService.UserId;

            var recommendations =
                await _recommendationService
                    .GetRecommendationsAsync(
                        userId,
                        location);

            ViewBag.Location = location;

            return View(recommendations);
        }
    }
}