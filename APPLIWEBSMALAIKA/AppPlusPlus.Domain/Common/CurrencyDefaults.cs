namespace AppPlusPlus.Domain.Common;

/// <summary>
/// Règles métier devises :
/// - Devise de stockage unique : Franc Congolais (FC / CDF) pour tous les montants en base.
/// - Dollar (USD) : conversion d'affichage uniquement (FC / taux actif).
/// - Id_Monais USD (1) et CDF (2) restent pour préférence d'affichage et quatuor historique.
/// </summary>
public static class CurrencyDefaults
{
    /// <summary>Id_Monais USD dans T_Moneys (affichage / conversion).</summary>
    public const int MoneyIdUsd = 1;

    /// <summary>Id_Monais CDF dans T_Moneys (devise de stockage).</summary>
    public const int MoneyIdCdf = 2;

    /// <summary>Devise par défaut pour tout nouvel enregistrement monétaire.</summary>
    public const int StorageMoneyId = MoneyIdCdf;

    public const string UsdCode = "USD";
    public const string CdfCode = "CDF";

    public static bool IsUsdMoneyId(int idMonais) => idMonais == MoneyIdUsd;

    public static bool IsCdfMoneyId(int idMonais) => idMonais == MoneyIdCdf;
}
