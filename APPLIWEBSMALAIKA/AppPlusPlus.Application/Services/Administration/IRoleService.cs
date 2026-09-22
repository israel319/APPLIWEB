namespace AppPlusPlus.Application.Services.Administration;

public interface IRoleService
{
    /// <summary>
    /// Initialize default permissions for all active roles that have no permissions yet.
    /// </summary>
    Task InitializeDefaultPermissionsAsync();

    /// <summary>
    /// Set default permissions for a specific role.
    /// If overwrite is true, existing permissions are removed first.
    /// </summary>
    Task SetDefaultPermissionsForRoleAsync(int roleId, bool overwrite = false);
}
