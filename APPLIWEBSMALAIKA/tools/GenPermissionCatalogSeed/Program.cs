using AppPlusPlus.Domain.Common;

var rows = PermissionCatalog.All
    .Select(n => (
        Code: n.Code.Replace("'", "''"),
        Label: PermissionCatalog.GetStorageLabel(n.Code).Replace("'", "''")))
    .ToList();

Console.WriteLine("-- Generated from PermissionCatalog — do not edit manually");
foreach (var (code, label) in rows)
    Console.WriteLine($"    (N'{code}', N'{label}'),");
