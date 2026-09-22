using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Application.Services.Administration;

public interface IPermissionResolver
{
    /// <summary>
    /// Resolves the effective permissions for a user login.
    /// First checks user-level activity grants, then falls back to role-based permissions.
    /// </summary>
    Task<PermissionSnapshot> GetPermissionsAsync(string? login);
}
