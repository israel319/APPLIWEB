using System.Globalization;
using System.Text;

namespace AppPlusPlus.Domain.Common;

public static class AppFunctions
{
    public const string Dashboard = "dashboard";
    public const string Vente = "vente";
    public const string Facturation = "facturation";
    public const string Stock = "stock";
    public const string Approvisionnement = "approvisionnement";
    public const string CommandesClients = "commandes clients";
    public const string CommandesInternes = "commandes internes";
    public const string Livraison = "livraison";
    public const string GRH = "grh";
    public const string Rapports = "rapports";
    public const string Services = "services";
    public const string Administration = "administration";
    public const string Parametres = "parametres";

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace("_", " ")
            .Replace("-", " ");
    }

    /// <summary>Rôle caissier de vente — pas d'accès au tableau de bord.</summary>
    public static bool IsVendeurRole(string? roleDescription) =>
        Normalize(roleDescription) is "vendeur" or "vendeuse";

    public static bool IsClientRole(string? roleDescription) =>
        Normalize(roleDescription) == "client";
}
