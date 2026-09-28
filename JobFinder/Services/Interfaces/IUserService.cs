using JobFinder.Models;

namespace JobFinder.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
    }
}