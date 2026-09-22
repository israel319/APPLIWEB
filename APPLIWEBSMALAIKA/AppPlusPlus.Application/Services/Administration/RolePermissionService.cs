using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Application.Services.Administration;

public interface IRolePermissionService
{
    Task<List<Role>> GetActiveRolesAsync();
    Task<List<Role>> GetAllRolesAsync();
    Task<Role?> GetRoleAsync(int roleId);
    Task<int> CreateRoleAsync(string name);
    Task UpdateRoleAsync(int roleId, string name, bool isActive);
    Task<IReadOnlySet<string>> GetGrantedCodesAsync(int roleId);
    Task SaveGrantedCodesAsync(int roleId, IEnumerable<string> codes);
    Task<IReadOnlyList<int>> ResolveCatalogActivityIdsAsync(IEnumerable<string> codes);
    Task EnsureCatalogActivitiesAsync();
    Task<Role> UpsertRoleByNameAsync(string name);
}

public class RolePermissionService : IRolePermissionService
{
    private readonly IRoleRepository _roleRepo;

    public RolePermissionService(IRoleRepository roleRepo)
    {
        _roleRepo = roleRepo;
    }

    public Task<List<Role>> GetActiveRolesAsync() => _roleRepo.GetActiveRolesAsync();

    public async Task<List<Role>> GetAllRolesAsync()
    {
        var roles = await _roleRepo.GetAllAsync();
        return roles.OrderBy(r => r.DescriptionRole).ToList();
    }

    public Task<Role?> GetRoleAsync(int roleId) => _roleRepo.GetByIdAsync(roleId);

    public async Task<int> CreateRoleAsync(string name)
    {
        var role = new Role
        {
            DescriptionRole = name.Trim(),
            IsActive = true
        };
        await _roleRepo.AddAsync(role);
        return role.RoleId;
    }

    public async Task UpdateRoleAsync(int roleId, string name, bool isActive)
    {
        var role = await _roleRepo.GetByIdAsync(roleId)
            ?? throw new InvalidOperationException("Rôle introuvable.");
        role.DescriptionRole = name.Trim();
        role.IsActive = isActive;
        await _roleRepo.UpdateAsync(role);
    }

    public async Task<IReadOnlySet<string>> GetGrantedCodesAsync(int roleId)
    {
        var rows = await _roleRepo.GetRoleActivitiesAsync(roleId);
        return rows
            .Where(r => r.Activity?.Code != null)
            .Select(r => PermissionCatalog.NormalizeCode(r.Activity!.Code))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task SaveGrantedCodesAsync(int roleId, IEnumerable<string> codes)
    {
        var ids = await ResolveCatalogActivityIdsAsync(codes);
        await _roleRepo.SetRoleActivitiesAsync(roleId, ids.ToList());
    }

    public async Task<IReadOnlyList<int>> ResolveCatalogActivityIdsAsync(IEnumerable<string> codes)
    {
        await EnsureCatalogActivitiesAsync();
        var normalized = codes
            .Select(PermissionCatalog.NormalizeCode)
            .Where(c => PermissionCatalog.Find(c) is not null)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
            return Array.Empty<int>();

        var activities = await _roleRepo.GetActivitiesByCodesAsync(normalized);
        var found = activities
            .Select(a => PermissionCatalog.NormalizeCode(a.Code))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = normalized.Where(c => !found.Contains(c)).ToList();

        if (missing.Count > 0)
        {
            await EnsureCatalogActivitiesAsync();
            activities = await _roleRepo.GetActivitiesByCodesAsync(normalized);
            found = activities
                .Select(a => PermissionCatalog.NormalizeCode(a.Code))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            missing = normalized.Where(c => !found.Contains(c)).ToList();
        }

        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"Activités introuvables pour : {string.Join(", ", missing)}. Exécutez Migration_Permissions_Hierarchiques.sql puis réessayez.");

        return activities
            .GroupBy(a => PermissionCatalog.NormalizeCode(a.Code), StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderBy(a => a.ActivityId).First().ActivityId)
            .ToList();
    }

    public async Task EnsureCatalogActivitiesAsync()
    {
        var nodes = PermissionCatalog.All
            .Select(n => (n.Code, n.Label))
            .ToList();
        await _roleRepo.SyncPermissionCatalogActivitiesAsync(nodes);
    }

    public async Task<Role> UpsertRoleByNameAsync(string name)
    {
        var trimmed = name.Trim();
        var roles = await _roleRepo.GetAllAsync();
        var existing = roles.FirstOrDefault(r =>
            string.Equals(r.DescriptionRole, trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
            return existing;

        var roleId = await CreateRoleAsync(trimmed);
        return (await _roleRepo.GetByIdAsync(roleId))!;
    }
}
