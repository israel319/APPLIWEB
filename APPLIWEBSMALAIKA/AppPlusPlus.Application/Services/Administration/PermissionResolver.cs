using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Application.Services.Administration;

public class PermissionResolver : IPermissionResolver
{
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;

    public PermissionResolver(IUserRepository userRepo, IRoleRepository roleRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
    }

    /// <inheritdoc />
    public async Task<PermissionSnapshot> GetPermissionsAsync(string? login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return PermissionSnapshot.Empty;

        var user = await _userRepo.GetWithRoleAsync(login);
        if (user?.RoleId == null || user.Activated != true)
            return PermissionSnapshot.Empty;

        var roleName = user.Role?.DescriptionRole;
        var grants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var userActivities = await _userRepo.GetUserActivitiesAsync(login);
        AddActivityCodes(userActivities.Where(ua => ua.IsGranted).Select(ua => ua.Activity?.Code), grants);

        // Droits hiérarchiques utilisateur (T_User_Activities) = source de vérité si présents.
        if (grants.Count > 0)
            return new PermissionSnapshot([], [], [], grants);

        grants.Clear();
        var roleActivities = await _roleRepo.GetRoleActivitiesAsync(user.RoleId.Value);
        AddActivityCodes(roleActivities.Where(ra => ra.IsGranted).Select(ra => ra.Activity?.Code), grants);

        if (grants.Count > 0)
            return new PermissionSnapshot([], [], [], grants);

        var read = new HashSet<string>();
        var write = new HashSet<string>();
        var delete = new HashSet<string>();

        ApplyRolePermissions(await _roleRepo.GetPermissionsByRoleAsync(user.RoleId.Value), read, write, delete);
        ApplyLegacyUserActivities(userActivities.Where(ua => ua.IsGranted), read, write, delete);

        var snapshot = new PermissionSnapshot(read, write, delete, grants);
        return AppFunctionResolver.EnrichWithDashboard(snapshot, roleName);
    }

    static void AddActivityCodes(IEnumerable<string?> codes, HashSet<string> grants)
    {
        foreach (var code in codes)
        {
            if (string.IsNullOrWhiteSpace(code)) continue;
            var norm = PermissionCatalog.NormalizeCode(code);
            if (PermissionCatalog.Find(norm) is not null)
                grants.Add(norm);
        }
    }

    private static void ApplyRolePermissions(
        IEnumerable<Permission> permissions,
        HashSet<string> read,
        HashSet<string> write,
        HashSet<string> delete)
    {
        foreach (var perm in permissions)
        {
            foreach (var module in AppFunctionResolver.ModulesFromPermission(
                         perm.FonctionId, perm.Fonction?.DescriptionFonction))
            {
                if (perm.CanRead)
                    read.Add(module);

                if (perm.CanWrite)
                {
                    read.Add(module);
                    write.Add(module);
                }

                if (perm.CanDelete)
                {
                    read.Add(module);
                    write.Add(module);
                    delete.Add(module);
                }
            }
        }
    }

    private static void ApplyLegacyUserActivities(
        IEnumerable<UserActivity> userActivities,
        HashSet<string> read,
        HashSet<string> write,
        HashSet<string> delete)
    {
        foreach (var userActivity in userActivities)
        {
            var activity = userActivity.Activity;
            if (string.IsNullOrWhiteSpace(activity?.Code))
                continue;

            var module = AppFunctionResolver.FromActivityCode(activity.Code);
            if (string.IsNullOrWhiteSpace(module))
                continue;

            var normalizedModule = AppFunctions.Normalize(module);
            var activityCode = activity.Code.Trim().ToLowerInvariant();

            if (activityCode.EndsWith("_read", StringComparison.Ordinal))
                read.Add(normalizedModule);
            else if (activityCode.EndsWith("_create", StringComparison.Ordinal) || activityCode.EndsWith("_update", StringComparison.Ordinal))
            {
                read.Add(normalizedModule);
                write.Add(normalizedModule);
            }
            else if (activityCode.EndsWith("_delete", StringComparison.Ordinal))
            {
                read.Add(normalizedModule);
                write.Add(normalizedModule);
                delete.Add(normalizedModule);
            }
        }
    }
}
