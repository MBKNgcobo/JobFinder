using JobFinder.Data;
using JobFinder.Models;
using JobFinder.Models.ViewModels;
using JobFinder.Security;
using JobFinder.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Controllers
{
    // All actions require an authenticated user. Project records are
    // additionally bound to their owner, and an administrator may
    // manage every project.
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        public ProjectsController(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        // GET: Projects
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _currentUserService.UserId;

            var projects = await _context.Projects
                .Include(p => p.ProjectSkills)
                    .ThenInclude(ps => ps.Skill)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            return View(projects);
        }

        // GET: Projects/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _currentUserService.UserId;

            if (userId <= 0)
            {
                return Unauthorized();
            }

            var model = new ProjectFormViewModel
            {
                UserId = userId,
                AvailableSkills = await _context.Skills
                    .OrderBy(s => s.Name)
                    .ToListAsync()
            };

            return View(model);
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectFormViewModel model)
        {
            var userId = _currentUserService.UserId;

            if (userId <= 0)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                await LoadSkills(model);
                return View(model);
            }

            var project = new Project
            {
                Name = model.Name,
                Description = model.Description,
                GitHubUrl = model.GitHubUrl,
                LiveUrl = model.LiveUrl,
                StartDate = model.StartDate,
                EndDate = model.EndDate,

                // The owner is always the authenticated user; a
                // posted UserId is never trusted.
                UserId = userId
            };

            _context.Projects.Add(project);

            await _context.SaveChangesAsync();

            // Add selected skills
            foreach (var skillId in model.SelectedSkillIds.Distinct())
            {
                _context.ProjectSkills.Add(
                    new ProjectSkill
                    {
                        ProjectId = project.Id,
                        SkillId = skillId
                    }
                );
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var project = await _context.Projects
                .Include(p => p.ProjectSkills)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            if (!CanManage(project))
                return Forbid();

            var model = new ProjectFormViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                GitHubUrl = project.GitHubUrl,
                LiveUrl = project.LiveUrl,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                UserId = project.UserId,

                SelectedSkillIds = project.ProjectSkills
                    .Select(ps => ps.SkillId)
                    .ToList(),

                AvailableSkills = await _context.Skills
                    .OrderBy(s => s.Name)
                    .ToListAsync()
            };

            return View(model);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProjectFormViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            var project = await _context.Projects
                .Include(p => p.ProjectSkills)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            // Ownership is checked before any other processing so a
            // foreign project is never disclosed or modified.
            if (!CanManage(project))
                return Forbid();

            if (!ModelState.IsValid)
            {
                // Never honour a client-supplied owner.
                model.UserId = project.UserId;

                await LoadSkills(model);
                return View(model);
            }

            project.Name = model.Name;
            project.Description = model.Description;
            project.GitHubUrl = model.GitHubUrl;
            project.LiveUrl = model.LiveUrl;
            project.StartDate = model.StartDate;
            project.EndDate = model.EndDate;

            // Remove old skills
            _context.ProjectSkills.RemoveRange(
                project.ProjectSkills
            );

            // Add new skills
            foreach (var skillId in model.SelectedSkillIds.Distinct())
            {
                project.ProjectSkills.Add(
                    new ProjectSkill
                    {
                        ProjectId = project.Id,
                        SkillId = skillId
                    }
                );
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var project = await _context.Projects
                .Include(p => p.User)
                .Include(p => p.ProjectSkills)
                    .ThenInclude(ps => ps.Skill)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            if (!CanManage(project))
                return Forbid();

            return View(project);
        }

        // POST: Projects/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectSkills)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            if (!CanManage(project))
                return Forbid();

            _context.ProjectSkills.RemoveRange(
                project.ProjectSkills
            );

            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // AUTHORIZATION HELPERS
        // ============================================================

        // A project may only be read or modified by the user who owns
        // it, or by an administrator.
        private bool CanManage(Project project)
        {
            if (_currentUserService.IsAdmin)
            {
                return true;
            }

            return project.UserId > 0 &&
                   project.UserId == _currentUserService.UserId;
        }

        private async Task LoadSkills(
            ProjectFormViewModel model)
        {
            model.AvailableSkills = await _context.Skills
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}