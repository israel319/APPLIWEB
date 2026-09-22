namespace AppPlusPlus.Domain.Common;

/// <summary>Normalisation et affichage des quantités — une seule règle pour toute l'application.</summary>
public static class QuantityFormat
{
    public const int StorageScale = 2;

    /// <summary>Arrondi stable à l'enregistrement (decimal 18,2 en base).</summary>
    public static decimal Normalize(decimal qte) =>
        Math.Round(qte, StorageScale, MidpointRounding.AwayFromZero);

    /// <summary>Affichage fidèle : 1500 reste 1500, 1499.50 affiche les décimales.</summary>
    public static string Display(decimal qte)
    {
        var normalized = Normalize(qte);
        return normalized == decimal.Truncate(normalized)
            ? normalized.ToString("0")
            : normalized.ToString("0.##");
    }
}
