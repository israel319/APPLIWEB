using System.Globalization;

namespace AppPlusPlus.Domain.Common;

/// <summary>Formatage des quantités (unités entières ou décimales, ex. poisson au poids).</summary>
public static class QtyFormat
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <summary>Affichage lisible (facture, détail) — virgule décimale, max 3 décimales.</summary>
    public static string Display(double qte, int maxDecimals = 3)
    {
        if (qte <= 0) return "0";
        var rounded = Math.Round(qte, maxDecimals, MidpointRounding.AwayFromZero);
        if (Math.Abs(rounded - Math.Round(rounded)) < 0.0001)
            return ((long)Math.Round(rounded)).ToString(Fr);
        return rounded.ToString("0.###", Fr);
    }

    public static string Display(double? qte, int maxDecimals = 3) =>
        Display(qte ?? 0, maxDecimals);

    /// <summary>Valeur pour champ de saisie HTML (point décimal, invariant).</summary>
    public static string InputValue(double qte, int maxDecimals = 3)
    {
        if (qte <= 0) return "1";
        var rounded = Math.Round(qte, maxDecimals, MidpointRounding.AwayFromZero);
        if (Math.Abs(rounded - Math.Round(rounded)) < 0.0001)
            return ((long)Math.Round(rounded)).ToString(Invariant);
        return rounded.ToString("0.###", Invariant);
    }
}
