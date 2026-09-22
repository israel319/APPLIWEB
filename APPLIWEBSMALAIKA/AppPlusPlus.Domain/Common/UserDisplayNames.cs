namespace AppPlusPlus.Domain.Common;

public static class UserDisplayNames
{
    public static string Format(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "—";

        if (value.Equals("_login", StringComparison.OrdinalIgnoreCase))
            return "Utilisateur";

        if (value.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase))
            return "Système";

        return value.Trim();
    }
}
