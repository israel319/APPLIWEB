namespace AppPlusPlus.Domain.Common;

/// <summary>
/// Règles métier des rôles (figées). Complète la matrice T_Permissions.
/// </summary>
public static class SystemRolePolicy
{
    public static bool IsAdministrateur(string? roleDescription) =>
        AppFunctions.Normalize(roleDescription) is "administrateur" or "admin";

    public static bool IsGerant(string? roleDescription) =>
        AppFunctions.Normalize(roleDescription) == "gerant";

    public static bool IsMagasinier(string? roleDescription) =>
        AppFunctions.Normalize(roleDescription) == "magasinier";

    /// <summary>Administrateur ou gérant : voit toutes les demandes.</summary>
    public static bool HasGlobalDemandeScope(string? roleDescription) =>
        IsAdministrateur(roleDescription) || IsGerant(roleDescription);

    /// <summary>Étape 1 — valider le prélèvement sur le dépôt.</summary>
    public static bool CanApproveDemandeDepot(string? roleDescription, PermissionSnapshot permissions) =>
        (IsAdministrateur(roleDescription) || IsGerant(roleDescription))
        && permissions.CanWrite(AppFunctions.CommandesInternes);

    /// <summary>Étape 2 — confirmer la réception sur sa localisation.</summary>
    public static bool CanConfirmDemandeMagasin(
        PermissionSnapshot permissions,
        int localisationDemandeurId,
        IReadOnlyList<int> userLocalisationIds) =>
        permissions.CanWrite(AppFunctions.CommandesInternes)
        && userLocalisationIds.Contains(localisationDemandeurId);
}
