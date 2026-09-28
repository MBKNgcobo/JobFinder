using JobFinder.Models.ViewModels;
using JobFinder.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobFinder.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        public ProfileController(
            IUserService userService,
            ICurrentUserService currentUserService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _currentUserService.UserId;

            if (userId <= 0)
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Summary = user.Summary,
                YearsOfExperience = user.YearsOfExperience,

                Skills = user.UserSkills
                    .Where(us => us.Skill != null)
                    .Select(us => us.Skill.Name)
                    .OrderBy(s => s)
                    .ToList(),

                Projects = user.Projects
                    .Select(p => new ProfileProjectViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        GitHubUrl = p.GitHubUrl,
                        LiveUrl = p.LiveUrl,

                        Skills = p.ProjectSkills
                            .Where(ps => ps.Skill != null)
                            .Select(ps => ps.Skill.Name)
                            .OrderBy(s => s)
                            .ToList()
                    })
                    .OrderBy(p => p.Name)
                    .ToList()
            };

            return View(model);
        }
    }
}