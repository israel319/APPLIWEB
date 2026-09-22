using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetWithPermissionsAsync(int roleId);
    Task<List<Role>> GetActiveRolesAsync();
    Task<List<Permission>> GetPermissionsByRoleAsync(int roleId);
    Task SetPermissionsAsync(int roleId, List<Permission> permissions);
    Task<List<Fonction>> GetAllFonctionsAsync();
    Task<List<Activity>> GetAllActivitiesAsync();
    Task<List<Activity>> GetActivitiesByFonctionAsync(int fonctionId);
    Task<List<RoleActivity>> GetRoleActivitiesAsync(int roleId);
    Task SetRoleActivitiesAsync(int roleId, IReadOnlyList<int> grantedActivityIds);
    Task<List<Activity>> GetActivitiesByCodesAsync(IEnumerable<string> codes);
    Task SyncPermissionCatalogActivitiesAsync(IReadOnlyList<(string Code, string Label)> nodes);
}
