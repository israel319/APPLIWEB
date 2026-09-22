namespace AppPlusPlus.Domain.Common;

/// <summary>
/// Fait le lien entre le schéma legacy (T_Fonctions : Facture, Article…)
/// et les modules web (vente, stock, facturation…).
/// </summary>
public static class AppFunctionResolver
{
    private static readonly string[] ActivitySuffixes = ["_read", "_create", "_update", "_delete"];

    /// <summary>Extrait le module depuis un code activité (ex. vente_read → vente).</summary>
    public static string FromActivityCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        var lower = code.Trim().ToLowerInvariant();
        foreach (var suffix in ActivitySuffixes)
        {
            if (lower.EndsWith(suffix, StringComparison.Ordinal))
                return lower[..^suffix.Length].Replace('_', ' ');
        }

        return string.Empty;
    }

    /// <summary>
    /// Modules applicatifs accordés par une permission rôle (lecture des lignes T_Permissions).
    /// </summary>
    public static IEnumerable<string> ModulesFromPermission(int fonctionId, string? descriptionFonction)
    {
        if (IsModernLabel(descriptionFonction))
        {
            yield return ModernLabelToModule(descriptionFonction!);
            yield break;
        }

        if (LegacyFonctionModules.TryGetValue(fonctionId, out var modules))
        {
            foreach (var module in modules)
                yield return module;
        }
    }

    public static bool ActivityMatchesPermission(
        string activityCode,
        int permissionFonctionId,
        string? permissionFonctionLabel)
    {
        var activityModule = AppFunctions.Normalize(FromActivityCode(activityCode));
        if (string.IsNullOrEmpty(activityModule))
            return false;

        return ModulesFromPermission(permissionFonctionId, permissionFonctionLabel)
            .Any(m => AppFunctions.Normalize(m) == activityModule);
    }

    public static PermissionSnapshot EnrichWithDashboard(PermissionSnapshot snapshot, string? roleName = null)
    {
        if (snapshot.HasHierarchicalGrants)
            return snapshot;

        if (AppFunctions.IsVendeurRole(roleName))
            return snapshot;

        if (!snapshot.CanRead(AppFunctions.Dashboard) && HasAnyModuleAccess(snapshot))
        {
            return snapshot.WithRead(AppFunctions.Dashboard);
        }

        return snapshot;
    }

    private static bool HasAnyModuleAccess(PermissionSnapshot snapshot) =>
        snapshot.CanRead(AppFunctions.Vente)
        || snapshot.CanRead(AppFunctions.Facturation)
        || snapshot.CanRead(AppFunctions.Stock)
        || snapshot.CanRead(AppFunctions.Approvisionnement)
        || snapshot.CanRead(AppFunctions.CommandesClients)
        || snapshot.CanRead(AppFunctions.CommandesInternes)
        || snapshot.CanRead(AppFunctions.Livraison)
        || snapshot.CanRead(AppFunctions.GRH)
        || snapshot.CanRead(AppFunctions.Rapports)
        || snapshot.CanRead(AppFunctions.Administration)
        || snapshot.CanRead(AppFunctions.Parametres);

    private static bool IsModernLabel(string? descriptionFonction)
    {
        var n = AppFunctions.Normalize(descriptionFonction);
        return n is "dashboard" or "vente" or "facturation" or "stock" or "approvisionnement"
            or "commandes clients" or "commandes internes" or "livraison" or "grh"
            or "rapports" or "administration" or "parametres";
    }

    private static string ModernLabelToModule(string descriptionFonction) =>
        AppFunctions.Normalize(descriptionFonction) switch
        {
            "dashboard" => AppFunctions.Dashboard,
            "vente" => AppFunctions.Vente,
            "facturation" => AppFunctions.Facturation,
            "stock" => AppFunctions.Stock,
            "approvisionnement" => AppFunctions.Approvisionnement,
            "commandes clients" => AppFunctions.CommandesClients,
            "commandes internes" => AppFunctions.CommandesInternes,
            "livraison" => AppFunctions.Livraison,
            "grh" => AppFunctions.GRH,
            "rapports" => AppFunctions.Rapports,
            "administration" => AppFunctions.Administration,
            "parametres" => AppFunctions.Parametres,
            _ => descriptionFonction
        };

    /// <summary>Mapping legacy T_Fonctions (Id + libellé historique) → modules web.</summary>
    private static readonly Dictionary<int, string[]> LegacyFonctionModules = new()
    {
        [1] = [AppFunctions.Vente, AppFunctions.Facturation, AppFunctions.Livraison],   // Facture
        [2] = [AppFunctions.Approvisionnement],                                          // Approvisionnement
        [3] = [AppFunctions.Stock],                                                      // Article
        [4] = [AppFunctions.Rapports],                                                   // Rapport
        [5] = [AppFunctions.Administration],                                             // Admin
        [6] = [AppFunctions.Stock],                                                      // Transformation
        [7] = [],                                                                          // Modification (droits transverses)
        [8] = [],                                                                          // Suppression
        [9] = [AppFunctions.CommandesClients, AppFunctions.CommandesInternes],           // Commande
        [10] = [AppFunctions.Livraison],                                                 // Livraison
        [11] = [AppFunctions.Facturation],                                               // Payement
    };
}
