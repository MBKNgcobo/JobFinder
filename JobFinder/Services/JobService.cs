using JobFinder.Data;
using JobFinder.Models;
using JobFinder.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Services
{
    public class JobService : IJobService
    {
        private readonly ApplicationDbContext _context;

        public JobService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Job>> GetAllJobsAsync(
            string? search = null,
            string? location = null,
            string? source = null,
            int? userId = null,
            decimal? minMatchScore = null)
        {
            var query = _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .AsQueryable();

            // --------------------------------------------------------
            // Keyword search
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();

                query = query.Where(j =>
                    j.Title.Contains(keyword) ||
                    j.Description.Contains(keyword) ||
                    (j.Company != null &&
                     j.Company.Name.Contains(keyword)));
            }

            // --------------------------------------------------------
            // Location filter
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(location))
            {
                var selectedLocation = location.Trim();

                query = query.Where(j =>
                    j.Location != null &&
                    j.Location.Contains(selectedLocation));
            }

            // --------------------------------------------------------
            // Source filter
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(source))
            {
                var selectedSource = source.Trim();

                query = query.Where(j =>
                    j.Source != null &&
                    j.Source.Contains(selectedSource));
            }

            // --------------------------------------------------------
            // Match-score filter
            // --------------------------------------------------------
            //
            // MatchScore belongs to JobMatch, which is user-specific.
            //
            // Therefore:
            // Job -> JobMatch -> User
            //
            // We only consider the logged-in user's match.
            //

            if (minMatchScore.HasValue)
            {
                if (!userId.HasValue || userId.Value <= 0)
                {
                    return new List<Job>();
                }

                var score = minMatchScore.Value;

                query = query.Where(j =>
                    _context.JobMatches.Any(
                        match =>
                            match.JobId == j.Id &&
                            match.UserId == userId.Value &&
                            match.MatchScore >= score));
            }

            // --------------------------------------------------------
            // Ordering
            // --------------------------------------------------------

            return await query
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();
        }

        public async Task<Job?> GetJobByIdAsync(int id)
        {
            return await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<Job> CreateJobAsync(Job job)
        {
            _context.Jobs.Add(job);

            await _context.SaveChangesAsync();

            return job;
        }

        public async Task<bool> UpdateJobAsync(Job job)
        {
            var existingJob = await _context.Jobs.FindAsync(job.Id);

            if (existingJob == null)
            {
                return false;
            }

            existingJob.Title = job.Title;
            existingJob.Description = job.Description;
            existingJob.Location = job.Location;
            existingJob.EmploymentType = job.EmploymentType;
            existingJob.SalaryMin = job.SalaryMin;
            existingJob.SalaryMax = job.SalaryMax;
            existingJob.PostedDate = job.PostedDate;
            existingJob.ClosingDate = job.ClosingDate;
            existingJob.Source = job.Source;
            existingJob.SourceUrl = job.SourceUrl;
            existingJob.CompanyId = job.CompanyId;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteJobAsync(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return false;
            }

            _context.Jobs.Remove(job);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}