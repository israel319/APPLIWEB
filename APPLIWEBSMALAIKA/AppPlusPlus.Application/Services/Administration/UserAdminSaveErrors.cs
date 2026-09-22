namespace AppPlusPlus.Application.Services.Administration;

/// <summary>Messages utilisateur pour les échecs création / édition utilisateur.</summary>
public static class UserAdminSaveErrors
{
    public static string Format(Exception ex)
    {
        if (ex is InvalidOperationException invalid)
            return invalid.Message;

        var msg = ex.InnerException?.Message ?? ex.Message;

        if (msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("dupliqu", StringComparison.OrdinalIgnoreCase))
        {
            return "Ce login existe déjà. Choisissez un autre identifiant ou modifiez l'utilisateur existant.";
        }

        if (msg.Contains("same key has already been added", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("An item with the same key", StringComparison.OrdinalIgnoreCase))
        {
            var key = ExtractDuplicateKey(msg);
            return string.IsNullOrWhiteSpace(key)
                ? "Doublons de permissions en base (T_Activities). Exécutez Fix_T_Activities_Duplicate_Codes.sql, redéployez l'application, puis réessayez."
                : $"Doublon en base pour la permission « {key} ». Exécutez Fix_T_Activities_Duplicate_Codes.sql, redéployez l'application, puis réessayez.";
        }

        if (msg.Contains("T_Role_Activities", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("T_User_Activities", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase))
        {
            return "Tables permissions manquantes en base. Exécutez Deploy_Permissions_Hierarchiques_Complet.sql puis réessayez.";
        }

        return msg.Length > 220 ? msg[..220] + "…" : msg;
    }

    public static string? ExtractDuplicateKey(string message)
    {
        const string marker = "Key:";
        var idx = message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        return message[(idx + marker.Length)..].Trim().TrimEnd('.');
    }
}
