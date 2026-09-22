using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class RoleRepository : RepositoryBase<Role>, IRoleRepository
{
    public RoleRepository(IDbContextFactory<AppDbContext> dbFactory) : base(dbFactory) { }

    public async Task<Role?> GetWithPermissionsAsync(int roleId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Roles
            .Include(r => r.Permissions).ThenInclude(p => p.Fonction)
            .FirstOrDefaultAsync(r => r.RoleId == roleId);
    }

    public async Task<List<Role>> GetActiveRolesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Roles.Where(r => r.IsActive).ToListAsync();
    }

    public async Task<List<Permission>> GetPermissionsByRoleAsync(int roleId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Permissions
            .Where(p => p.RoleId == roleId)
            .Include(p => p.Fonction)
            .ToListAsync();
    }

    public async Task SetPermissionsAsync(int roleId, List<Permission> permissions)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var existing = await ctx.Permissions.Where(p => p.RoleId == roleId).ToListAsync();
        ctx.Permissions.RemoveRange(existing);
        foreach (var perm in permissions)
        {
            perm.RoleId = roleId;
            ctx.Permissions.Add(perm);
        }
        await ctx.SaveChangesAsync();
    }

    public async Task<List<Fonction>> GetAllFonctionsAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Fonctions.ToListAsync();
    }

    public async Task<List<Activity>> GetAllActivitiesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Activities.Include(a => a.Fonction).ToListAsync();
    }

    public async Task<List<Activity>> GetActivitiesByFonctionAsync(int fonctionId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Activities.Where(a => a.FonctionId == fonctionId).ToListAsync();
    }

    public async Task<List<RoleActivity>> GetRoleActivitiesAsync(int roleId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.RoleActivities
            .Where(ra => ra.RoleId == roleId && ra.IsGranted)
            .Include(ra => ra.Activity)
            .ToListAsync();
    }

    public async Task SetRoleActivitiesAsync(int roleId, IReadOnlyList<int> grantedActivityIds)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var existing = await ctx.RoleActivities.Where(ra => ra.RoleId == roleId).ToListAsync();
        ctx.RoleActivities.RemoveRange(existing);

        foreach (var activityId in grantedActivityIds.Distinct())
        {
            ctx.RoleActivities.Add(new RoleActivity
            {
                RoleId = roleId,
                ActivityId = activityId,
                IsGranted = true
            });
        }

        await ctx.SaveChangesAsync();
    }

    public async Task<List<Activity>> GetActivitiesByCodesAsync(IEnumerable<string> codes)
    {
        var normalized = codes.Select(PermissionCatalog.NormalizeCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var all = await ctx.Activities.Where(a => a.IsActive).ToListAsync();
        return all.Where(a => normalized.Contains(PermissionCatalog.NormalizeCode(a.Code))).ToList();
    }

    public async Task SyncPermissionCatalogActivitiesAsync(IReadOnlyList<(string Code, string Label)> nodes)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var existing = await ctx.Activities.ToListAsync();
        var byCode = new Dictionary<string, Activity>(StringComparer.OrdinalIgnoreCase);
        foreach (var activity in existing)
        {
            if (string.IsNullOrWhiteSpace(activity.Code))
                continue;

            var norm = PermissionCatalog.NormalizeCode(activity.Code);
            if (!byCode.ContainsKey(norm))
                byCode[norm] = activity;
        }

        var adminFonctionId = await ctx.Fonctions
            .Select(f => f.IdFonction)
            .FirstOrDefaultAsync();

        if (adminFonctionId == 0)
            throw new InvalidOperationException(
                "Aucune fonction en base (T_Fonctions). Impossible de synchroniser le catalogue des permissions.");

        foreach (var (code, label) in nodes)
        {
            var norm = PermissionCatalog.NormalizeCode(code);
            if (byCode.ContainsKey(norm)) continue;

            var activity = new Activity
            {
                FonctionId = adminFonctionId,
                Code = norm,
                DescriptionActivity = PermissionCatalog.GetStorageLabel(norm),
                IsActive = true
            };
            ctx.Activities.Add(activity);
            byCode[norm] = activity;
        }

        await ctx.SaveChangesAsync();
    }
}
