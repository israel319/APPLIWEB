namespace AppPlusPlus.Domain.Common;

/// <summary>
/// Point d'entrée unique pour les contrôles d'accès.
/// En mode hiérarchique (T_Role_Activities), seuls les grants explicites comptent — aucun bypass admin/legacy.
/// </summary>
public static class PermissionGate
{
    public static bool IsStrict(PermissionSnapshot p) => p.HasHierarchicalGrants;

    public static bool ViewTab(PermissionSnapshot p, string tabCode, Func<bool> legacyFallback) =>
        p.HasHierarchicalGrants ? p.CanAccessTab(tabCode) : legacyFallback();

    public static bool ViewSubTab(PermissionSnapshot p, string subTabCode, Func<bool> legacyFallback) =>
        p.HasHierarchicalGrants ? p.CanAccessSubTab(subTabCode) : legacyFallback();

    public static bool DoAction(PermissionSnapshot p, string actionCode, Func<bool> legacyFallback) =>
        p.HasHierarchicalGrants ? p.CanPerformAction(actionCode) : legacyFallback();

    public static bool LegacyRead(PermissionSnapshot p, string module, bool isAdmin) =>
        isAdmin || p.CanRead(module);

    public static bool LegacyWrite(PermissionSnapshot p, string module, bool isAdmin) =>
        isAdmin || p.CanWrite(module);

    public static bool LegacyDelete(PermissionSnapshot p, string module, bool isAdmin) =>
        isAdmin || p.CanDelete(module);
}
