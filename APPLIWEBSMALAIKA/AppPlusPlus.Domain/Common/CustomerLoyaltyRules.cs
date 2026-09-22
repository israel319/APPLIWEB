namespace AppPlusPlus.Domain.Common;

/// <summary>
/// Règles de fidélité : points gagnés sur les commandes clients.
/// </summary>
public static class CustomerLoyaltyRules
{
    /// <summary>Montant FC cumulé pour 1 point de fidélité.</summary>
    public const decimal FcPerPoint = 1000m;

    public const string RuleSummary =
        "1 point de fidélité pour chaque 1 000 FC de commande livrée ou facturée.";

    public static int PointsFromAmount(decimal montantFc)
    {
        if (montantFc <= 0)
            return 0;

        return (int)Math.Floor(montantFc / FcPerPoint);
    }

    public static bool IsAcquiredStatus(int? status) =>
        (status ?? 0) >= 2;

    public static bool IsPendingStatus(int? status)
    {
        var st = status ?? 0;
        return st is 0 or 1;
    }
}
