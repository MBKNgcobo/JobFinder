using JobFinder.Data;
using JobFinder.Models;
using JobFinder.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.UserSkills)
                    .ThenInclude(us => us.Skill)
                .Include(u => u.Projects)
                    .ThenInclude(p => p.ProjectSkills)
                        .ThenInclude(ps => ps.Skill)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}