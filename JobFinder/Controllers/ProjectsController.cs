using JobFinder.Data;
using JobFinder.Models;
using JobFinder.Models.ViewModels;
using JobFinder.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Controllers
{
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
        public async Task<IActionResult> Index(int userId )
        {
            userId = _currentUserService.UserId;
            var projects = await _context.Projects
                .Include(p => p.ProjectSkills)
                    .ThenInclude(ps => ps.Skill)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            return View(projects);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create(int userId)
        {
            userId = _currentUserService.UserId;
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
                UserId = model.UserId
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

            return RedirectToAction(
                nameof(Index),
                new { userId = project.UserId }
            );
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var project = await _context.Projects
                .Include(p => p.ProjectSkills)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

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

            if (!ModelState.IsValid)
            {
                await LoadSkills(model);
                return View(model);
            }

            var project = await _context.Projects
                .Include(p => p.ProjectSkills)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

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

            return RedirectToAction(
                nameof(Index),
                new { userId = project.UserId }
            );
        }

        // GET: Projects/Details/5
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

            var userId = project.UserId;

            _context.ProjectSkills.RemoveRange(
                project.ProjectSkills
            );

            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index),
                new { userId }
            );
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