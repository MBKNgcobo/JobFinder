using JobFinder.Data;
using JobFinder.Models;
using JobFinder.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Application>>
      GetUserApplicationsAsync(int userId)
        {
            return await _context.Applications
                .Include(a => a.Job)
                .ThenInclude(j => j.Company)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.UpdatedAt)
                .ToListAsync();
        }

        public async Task<Application?> GetApplicationByIdAsync(int id)
        {
            return await _context.Applications
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .Include(a => a.Job)
                    .ThenInclude(j => j.JobSkills)
                        .ThenInclude(js => js.Skill)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Application?>
            CreateApplicationAsync(
                int userId,
                int jobId)
        {
            var existingApplication =
                await _context.Applications
                    .FirstOrDefaultAsync(
                        a =>
                            a.UserId == userId &&
                            a.JobId == jobId);

            if (existingApplication != null)
            {
                return existingApplication;
            }

            var job =
                await _context.Jobs
                    .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
            {
                return null;
            }

            var application = new Application
            {
                UserId = userId,
                JobId = jobId,
                Status = "Draft",
                UpdatedAt = DateTime.UtcNow
            };

            _context.Applications.Add(application);

            await _context.SaveChangesAsync();

            return await GetApplicationByIdAsync(
                application.Id);
        }

        public async Task<bool> UpdateStatusAsync(int applicationId,string status)
        {
            var application =
                await _context.Applications
                    .FirstOrDefaultAsync(
                        a => a.Id == applicationId);

            if (application == null)
            {
                return false;
            }

            application.Status = status;
            application.UpdatedAt = DateTime.UtcNow;

            if (status == "Applied")
            {
                application.AppliedAt ??= DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateApplicationAsync(Application application)
        {
            try
            {
                _context.Applications.Update(application);

                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}