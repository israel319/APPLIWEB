namespace AppPlusPlus.Domain.Common;

public sealed class PermissionSnapshot
{
    public static PermissionSnapshot Empty { get; } = new([], [], [], []);

    private readonly HashSet<string> _read;
    private readonly HashSet<string> _write;
    private readonly HashSet<string> _delete;
    private readonly HashSet<string> _grants;

    public PermissionSnapshot(
        IEnumerable<string> read,
        IEnumerable<string> write,
        IEnumerable<string> delete,
        IEnumerable<string>? grants = null)
    {
        _read = new HashSet<string>(read.Select(AppFunctions.Normalize));
        _write = new HashSet<string>(write.Select(AppFunctions.Normalize));
        _delete = new HashSet<string>(delete.Select(AppFunctions.Normalize));
        _grants = new HashSet<string>(
            (grants ?? []).Select(PermissionCatalog.NormalizeCode),
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlySet<string> Grants => _grants;

    public bool HasHierarchicalGrants => _grants.Count > 0;

    public bool CanRead(string functionName) => _read.Contains(AppFunctions.Normalize(functionName));
    public bool CanWrite(string functionName) => _write.Contains(AppFunctions.Normalize(functionName));
    public bool CanDelete(string functionName) => _delete.Contains(AppFunctions.Normalize(functionName));

    public bool CanAccessTab(string tabCode) => PermissionCatalog.CanAccessTab(_grants, tabCode);
    public bool CanAccessSubTab(string subTabCode) =>
        PermissionCatalog.CanAccessSubTabOrFullTab(_grants, subTabCode);
    public bool CanPerformAction(string actionCode) => PermissionCatalog.CanPerformAction(_grants, actionCode);
    public bool HasGrant(string code) => PermissionCatalog.IsGranted(_grants, code);

    public string HomeRoute() => PermissionCatalog.FirstAccessibleRoute(_grants);
    public string SafeRoute(string targetRoute) => PermissionCatalog.ResolveSafeRoute(_grants, targetRoute);
    public bool CanAccessRoute(string path) =>
        PermissionCatalog.CanAccessRoutePath(_grants, PermissionCatalog.NormalizeRoutePath(path));

    public PermissionSnapshot WithRead(string functionName)
    {
        var read = _read.ToHashSet();
        read.Add(AppFunctions.Normalize(functionName));
        return new PermissionSnapshot(read, _write, _delete, _grants);
    }
}
