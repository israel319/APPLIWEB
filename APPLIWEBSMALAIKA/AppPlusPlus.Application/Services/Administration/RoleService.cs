using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Application.Services.Administration;

/// <summary>
/// Service for initializing default permissions for standard roles.
/// </summary>
public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepo;

    public RoleService(IRoleRepository roleRepo)
    {
        _roleRepo = roleRepo;
    }

    /// <inheritdoc />
    public async Task InitializeDefaultPermissionsAsync()
    {
        var roles = await _roleRepo.GetActiveRolesAsync();
        var fonctions = await _roleRepo.GetAllFonctionsAsync();

        foreach (var role in roles)
        {
            var existing = await _roleRepo.GetPermissionsByRoleAsync(role.RoleId);
            if (existing.Any()) continue;

            var defaultPerms = GetDefaultPermissionsForRole(role.DescriptionRole, fonctions);
            if (defaultPerms.Any())
            {
                foreach (var perm in defaultPerms)
                    perm.RoleId = role.RoleId;

                await _roleRepo.SetPermissionsAsync(role.RoleId, defaultPerms);
            }
        }
    }

    /// <inheritdoc />
    public async Task SetDefaultPermissionsForRoleAsync(int roleId, bool overwrite = false)
    {
        var role = await _roleRepo.GetByIdAsync(roleId);
        if (role == null) return;

        if (overwrite)
        {
            // SetPermissionsAsync replaces all permissions for the role
            var fonctions = await _roleRepo.GetAllFonctionsAsync();
            var defaultPerms = GetDefaultPermissionsForRole(role.DescriptionRole, fonctions);

            foreach (var perm in defaultPerms)
                perm.RoleId = roleId;

            await _roleRepo.SetPermissionsAsync(roleId, defaultPerms);
        }
        else
        {
            var existing = await _roleRepo.GetPermissionsByRoleAsync(roleId);
            if (existing.Any()) return;

            var fonctions = await _roleRepo.GetAllFonctionsAsync();
            var defaultPerms = GetDefaultPermissionsForRole(role.DescriptionRole, fonctions);

            foreach (var perm in defaultPerms)
                perm.RoleId = roleId;

            await _roleRepo.SetPermissionsAsync(roleId, defaultPerms);
        }
    }

    // ----------------------------------------------------------------
    //  Permission matrix helpers (migrated from RolePermissionService)
    // ----------------------------------------------------------------

    private List<Permission> GetDefaultPermissionsForRole(string? roleName, List<Fonction> fonctions)
    {
        var permissions = new List<Permission>();
        var normalizedRole = NormalizeRoleName(roleName);

        foreach (var fonction in fonctions)
        {
            var perm = GetPermissionForRoleAndFonction(normalizedRole, fonction);
            if (perm != null)
                permissions.Add(perm);
        }

        return permissions;
    }

    private static string NormalizeRoleName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "";
        return name.ToLowerInvariant()
            .Replace("\u00e9", "e")   // e with acute
            .Replace("\u00e8", "e")   // e with grave
            .Replace("\u00ea", "e")   // e with circumflex
            .Replace("\u00e0", "a")   // a with grave
            .Replace("\u00e2", "a")   // a with circumflex
            .Replace("\u00ee", "i")   // i with circumflex
            .Replace("\u00f4", "o")   // o with circumflex
            .Replace("\u00fb", "u")   // u with circumflex
            .Trim();
    }

    private static Permission? GetPermissionForRoleAndFonction(string normalizedRole, Fonction fonction)
    {
        var fn = NormalizeRoleName(fonction.DescriptionFonction);

        return normalizedRole switch
        {
            "administrateur" or "admin" => PermRWD(fonction),

            "gerant" => fn switch
            {
                "dashboard"           => PermR(fonction),
                "vente"               => PermRWD(fonction),
                "facturation"         => PermRWD(fonction),
                "livraison"           => PermRWD(fonction),
                "stock"               => PermRWD(fonction),
                "approvisionnement"   => PermRWD(fonction),
                "commandes clients"   => PermRWD(fonction),
                "commandes internes"  => PermRWD(fonction),
                "services"            => PermRWD(fonction),
                "rapports"            => PermR(fonction),
                "parametres"          => PermR(fonction),
                _                     => null
            },

            "caissier" or "caissiere" => fn switch
            {
                "dashboard"           => PermR(fonction),
                "vente"               => PermRW(fonction),
                "facturation"         => PermRW(fonction),
                "livraison"           => PermR(fonction),
                "stock"               => PermR(fonction),
                "commandes clients"   => PermR(fonction),
                "services"            => PermR(fonction),
                "parametres"          => PermR(fonction),
                _                     => null
            },

            "magasinier" => fn switch
            {
                "dashboard"           => PermR(fonction),
                "vente"               => PermR(fonction),
                "livraison"           => PermRW(fonction),
                "stock"               => PermRWD(fonction),
                "approvisionnement"   => PermRWD(fonction),
                "commandes clients"   => PermR(fonction),
                "commandes internes"  => PermRWD(fonction),
                "rapports"            => PermR(fonction),
                "parametres"          => PermR(fonction),
                _                     => null
            },

            "vendeur" or "vendeuse" => fn switch
            {
                "vente"               => PermR(fonction),
                "facturation"         => PermRW(fonction),
                "livraison"           => PermR(fonction),
                "stock"               => PermR(fonction),
                "commandes clients"   => PermRW(fonction),
                "parametres"          => PermR(fonction),
                _                     => null
            },

            _ => null
        };
    }

    private static Permission PermR(Fonction f) => new()
        { FonctionId = f.IdFonction, CanRead = true,  CanWrite = false, CanDelete = false };

    private static Permission PermRW(Fonction f) => new()
        { FonctionId = f.IdFonction, CanRead = true,  CanWrite = true,  CanDelete = false };

    private static Permission PermRWD(Fonction f) => new()
        { FonctionId = f.IdFonction, CanRead = true,  CanWrite = true,  CanDelete = true  };
}
