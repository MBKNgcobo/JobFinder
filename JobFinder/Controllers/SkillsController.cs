using JobFinder.Data;
using JobFinder.Models;
using JobFinder.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Controllers
{
    // Shared reference data. Any authenticated user may browse
    // skills, but only an administrator may change them.
    [Authorize]
    public class SkillsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SkillsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Skills
        public async Task<IActionResult> Index()
        {
            var skills = await _context.Skills
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(skills);
        }

        // GET: Skills/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var skill = await _context.Skills
                .Include(s => s.UserSkills)
                .Include(s => s.JobSkills)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (skill == null)
                return NotFound();

            return View(skill);
        }

        // POST: Skills/Create
        [Authorize(Roles = AppRoles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Skill skill)
        {
            if (!ModelState.IsValid)
                return View(skill);

            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}