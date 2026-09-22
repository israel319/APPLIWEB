using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using AppPlusPlus.Application.Interfaces;

namespace AppPlusPlus.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? Login => User?.FindFirstValue("login");
    public int? RoleId => int.TryParse(User?.FindFirstValue("roleId"), out var id) ? id : null;
    public string? RoleName => User?.FindFirstValue(ClaimTypes.Role);
    public bool IsAdmin => string.Equals(RoleName, "Admin", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(RoleName, "Administrateur", StringComparison.OrdinalIgnoreCase);
}
