using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace AppPlusPlus.Application.Services.Administration;

public class UserAdminService : IUserAdminService
{
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IRolePermissionService _rolePermissionService;
    private readonly ILogger<UserAdminService> _logger;

    public UserAdminService(
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IRolePermissionService rolePermissionService,
        ILogger<UserAdminService> logger)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _rolePermissionService = rolePermissionService;
        _logger = logger;
    }

    public async Task<User?> GetUserByLoginAsync(string login)
    {
        return await _userRepo.GetByLoginAsync(login);
    }

    public async Task<User?> GetUserWithRoleAsync(string login)
    {
        return await _userRepo.GetWithRoleAsync(login);
    }

    public async Task<bool> UserExistsAsync(string login)
    {
        var normalized = login.Trim();
        var user = await _userRepo.GetByLoginAsync(normalized);
        return user != null;
    }

    public async Task<List<Role>> GetActiveRolesAsync()
    {
        return await _roleRepo.GetActiveRolesAsync();
    }

    public async Task<List<Activity>> GetActiveActivitiesWithFonctionAsync()
    {
        var all = await _roleRepo.GetAllActivitiesAsync();
        return all.Where(a => a.IsActive).ToList();
    }

    public async Task<List<int>> GetUserGrantedActivityIdsAsync(string login)
    {
        var activities = await _userRepo.GetUserActivitiesAsync(login);
        return activities
            .Where(ua => ua.IsGranted)
            .Select(ua => ua.ActivityId)
            .ToList();
    }

    /// <summary>
    /// Maps role permissions to activity IDs based on suffix patterns:
    /// CanRead  -> activities whose Code ends with "_read"
    /// CanWrite -> activities whose Code ends with "_create" or "_update"
    /// CanDelete -> activities whose Code ends with "_delete"
    /// Matching is done by FonctionId (permission and activity share the same Fonction).
    /// </summary>
    public async Task<HashSet<int>> GetRoleDefaultActivityIdsAsync(int? roleId)
    {
        if (roleId is null)
            return new HashSet<int>();

        var permissions = await _roleRepo.GetPermissionsByRoleAsync(roleId.Value);
        var allActivities = await _roleRepo.GetAllActivitiesAsync();
        var fonctions = await _roleRepo.GetAllFonctionsAsync();

        var result = new HashSet<int>();

        foreach (var perm in permissions)
        {
            var fonctionLabel = fonctions.FirstOrDefault(f => f.IdFonction == perm.FonctionId)?.DescriptionFonction;

            foreach (var act in allActivities.Where(a => a.IsActive))
            {
                if (!AppFunctionResolver.ActivityMatchesPermission(
                        act.Code ?? string.Empty,
                        perm.FonctionId,
                        fonctionLabel))
                    continue;

                var code = act.Code ?? string.Empty;

                if (perm.CanRead && code.EndsWith("_read", StringComparison.OrdinalIgnoreCase))
                    result.Add(act.ActivityId);

                if (perm.CanWrite &&
                    (code.EndsWith("_create", StringComparison.OrdinalIgnoreCase) ||
                     code.EndsWith("_update", StringComparison.OrdinalIgnoreCase)))
                    result.Add(act.ActivityId);

                if (perm.CanDelete && code.EndsWith("_delete", StringComparison.OrdinalIgnoreCase))
                    result.Add(act.ActivityId);
            }
        }

        return result;
    }

    public async Task CreateUserAsync(User user, HashSet<int> localisationIds)
    {
        await _userRepo.AddAsync(user);

        if (localisationIds.Count > 0)
        {
            await _userRepo.UpdateUserLocalisationsAsync(user.Login, localisationIds);
        }
    }

    public async Task UpdateUserAsync(User user, HashSet<int> localisationIds)
    {
        if (string.IsNullOrWhiteSpace(user.Password))
        {
            var existing = await _userRepo.GetByLoginAsync(user.Login);
            if (existing is not null)
                user.Password = existing.Password;
        }

        await _userRepo.UpdateAsync(user);
        await _userRepo.UpdateUserLocalisationsAsync(user.Login, localisationIds);
    }

    /// <summary>
    /// Updates the user's RoleId and replaces all UserActivity records.
    /// </summary>
    public async Task SaveUserAccessAsync(string login, int? roleId, List<int> grantedActivityIds)
    {
        var user = await _userRepo.GetByLoginAsync(login);
        if (user is null) return;

        user.RoleId = roleId;
        await _userRepo.UpdateAsync(user);

        await _userRepo.ReplaceUserActivitiesAsync(login, grantedActivityIds);
    }

    public async Task UpdateProfileAsync(string login, string name, string email)
    {
        var user = await _userRepo.GetByLoginAsync(login);
        if (user is null) return;

        user.Name = name;
        user.Email = email;
        await _userRepo.UpdateAsync(user);
    }

    public async Task<bool> ChangePasswordAsync(string login, string currentPassword, string newPassword)
    {
        var user = await _userRepo.GetByLoginAsync(login);
        if (user is null) return false;

        // Verify current password matches
        if (user.Password != currentPassword)
            return false;

        user.Password = newPassword;
        await _userRepo.UpdateAsync(user);
        return true;
    }

    public async Task<List<UserActivity>> GetUserActivitiesWithDetailsAsync(string login)
    {
        return await _userRepo.GetUserActivitiesAsync(login);
    }

    public async Task<List<User>> GetUsersByLocalisationAsync(int localisationId)
    {
        return await _userRepo.GetByLocalisationAsync(localisationId);
    }

    public async Task DeleteUserAsync(string login)
    {
        await _userRepo.DeleteUserAsync(login);
    }

    public async Task<IReadOnlySet<string>> GetUserPermissionCodesAsync(string login)
    {
        var activities = await _userRepo.GetUserActivitiesAsync(login);
        var fromUser = activities
            .Where(ua => ua.IsGranted && !string.IsNullOrWhiteSpace(ua.Activity?.Code))
            .Select(ua => PermissionCatalog.NormalizeCode(ua.Activity!.Code))
            .Where(c => PermissionCatalog.Find(c) is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (fromUser.Count > 0)
            return fromUser;

        var user = await _userRepo.GetWithRoleAsync(login);
        if (user?.RoleId is int roleId)
            return await _rolePermissionService.GetGrantedCodesAsync(roleId);

        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task SaveUserProfileAccessAsync(
        User user,
        string profileName,
        HashSet<int> localisationIds,
        IEnumerable<string> permissionCodes)
    {
        try
        {
            await _rolePermissionService.EnsureCatalogActivitiesAsync();

            var activityIds = await _rolePermissionService.ResolveCatalogActivityIdsAsync(permissionCodes);
            var role = await ResolveDedicatedRoleAsync(user, profileName);

            await _userRepo.ReplaceUserActivitiesAsync(user.Login, activityIds.ToList());

            await _rolePermissionService.SaveGrantedCodesAsync(role.RoleId, permissionCodes);
            await _roleRepo.SetPermissionsAsync(role.RoleId, new List<Permission>());

            user.RoleId = role.RoleId;
            await _userRepo.UpdateAsync(user);
            await _userRepo.UpdateUserLocalisationsAsync(user.Login, localisationIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SaveUserProfileAccessAsync failed for login {Login}, profile {Profile}",
                user.Login, profileName);
            throw;
        }
    }

    /// <summary>
    /// Chaque utilisateur possède son propre rôle en base.
    /// Si plusieurs utilisateurs partagent encore le même RoleId (legacy), on duplique le rôle.
    /// </summary>
    async Task<Role> ResolveDedicatedRoleAsync(User user, string profileName)
    {
        var trimmed = profileName.Trim();

        if (user.RoleId is int roleId)
        {
            var usersOnRole = await _userRepo.GetByRoleAsync(roleId);
            var soleOwner = usersOnRole.Count == 1
                && string.Equals(usersOnRole[0].Login, user.Login, StringComparison.OrdinalIgnoreCase);

            if (soleOwner)
            {
                var role = await _roleRepo.GetByIdAsync(roleId)
                    ?? throw new InvalidOperationException("Rôle utilisateur introuvable.");
                role.DescriptionRole = trimmed;
                await _roleRepo.UpdateAsync(role);
                return role;
            }
        }

        var newRoleId = await _rolePermissionService.CreateRoleAsync(trimmed);
        return (await _roleRepo.GetByIdAsync(newRoleId))!;
    }
}
