using JobFinder.Security;
using JobFinder.Services.Interfaces;
using System.Security.Claims;

namespace JobFinder.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId
        {
            get
            {
                var claimValue =
                    _httpContextAccessor.HttpContext?
                        .User?
                        .FindFirstValue(ClaimTypes.NameIdentifier);

                if (int.TryParse(claimValue, out var userId))
                {
                    return userId;
                }

                return 0;
            }
        }

        public bool IsAdmin =>
            _httpContextAccessor.HttpContext?
                .User?
                .IsInRole(AppRoles.Admin) == true;
    }
}