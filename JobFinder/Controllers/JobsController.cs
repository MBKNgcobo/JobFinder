using JobFinder.Models;
using JobFinder.Models.ViewModels;
using JobFinder.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobFinder.Controllers
{
    [Authorize]
    public class JobsController : Controller
    {
        private readonly IJobService _jobService;
        private readonly ICurrentUserService _currentUserService;

        public JobsController(
            IJobService jobService,
            ICurrentUserService currentUserService)
        {
            _jobService = jobService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? location,
            string? source,
            int page = 1)
        {
            if (page < 1)
            {
                page = 1;
            }

            const int pageSize = 10;

            var userId = _currentUserService.UserId;

            var jobs = await _jobService.GetAllJobsAsync(
                search: search,
                location: location,
                source: source,
                userId: userId > 0 ? userId : null);

            var totalJobs = jobs.Count;

            var pagedJobs = jobs
                .OrderByDescending(job => job.PostedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new JobSearchViewModel
            {
                Search = search,
                Location = location,
                Source = source,
                Page = page,
                PageSize = pageSize,
                TotalJobs = totalJobs,
                Jobs = pagedJobs
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }
    }
}