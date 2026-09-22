using System.Globalization;

namespace AppPlusPlus.Domain.Common;

/// <summary>
/// Formatage centralisé des montants — FC (franc congolais) et USD (dollar).
/// Règle métier : le FC est la seule devise de stockage en base de données.
/// Le USD est une conversion d'affichage : USD = FC / taux actif.
/// </summary>
public static class CurrencyFormat
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    public static bool IsUsd(string? moneyDescription) =>
        string.Equals(moneyDescription?.Trim(), "USD", StringComparison.OrdinalIgnoreCase);

    public static bool IsUsd(int idMonais, IReadOnlyDictionary<int, string?> moneyById) =>
        moneyById.TryGetValue(idMonais, out var desc) && IsUsd(desc);

    /// <summary>Arrondi FC à l'unité supérieure (règle métier prix catalogue / vente).</summary>
    public static decimal RoundFcUp(decimal fc) =>
        fc <= 0 ? fc : Math.Ceiling(fc);

    /// <summary>Montant natif (USD ou FC) → équivalent FC entier, arrondi par excès.</summary>
    public static decimal ToCdf(decimal amountNative, bool isUsd, decimal taux)
    {
        if (amountNative <= 0)
            return 0;

        var fc = isUsd && taux > 0 ? amountNative * taux : amountNative;
        fc = RoundFcUp(fc);
        if (fc <= 0)
            fc = 1;
        return fc;
    }

    /// <summary>Prix unitaire article → FC (alias explicite pour le catalogue et la caisse).</summary>
    public static decimal ArticleToFc(decimal amountNative, bool isUsd, decimal taux) =>
        ToCdf(amountNative, isUsd, taux);

    /// <summary>Total ligne article (PU × Qté) en FC, arrondi par excès sur le PU.</summary>
    public static decimal ArticleLineToFc(decimal unitNative, decimal qty, bool isUsd, decimal taux)
    {
        if (unitNative <= 0 || qty <= 0)
            return 0;
        return ArticleToFc(unitNative, isUsd, taux) * qty;
    }

    /// <summary>Montant natif (USD ou FC) → équivalent USD.</summary>
    public static decimal ToUsd(decimal amountNative, bool isUsd, decimal taux) =>
        !isUsd && taux > 0 ? Math.Round(amountNative / taux, 2)
        : isUsd ? amountNative : 0;

    /// <summary>Décompose un montant natif en paire (USD, FC).</summary>
    public static (decimal Usd, decimal Fc) Split(decimal amountNative, bool isUsd, decimal taux)
    {
        if (taux <= 0)
            return isUsd ? (amountNative, amountNative) : (0, amountNative);
        return isUsd
            ? (amountNative, ToCdf(amountNative, true, taux))
            : (Math.Round(amountNative / taux, 2), RoundFcUp(amountNative));
    }

    /// <summary>Convertit un montant FC en montant affiché selon la devise de la facture.</summary>
    public static decimal ToDisplayAmount(decimal amountFc, bool isUsd, decimal taux) =>
        isUsd && taux > 0 ? Math.Round(NormalizeLegacyFactCdf(amountFc, taux) / taux, 2)
        : NormalizeLegacyFactCdf(amountFc, taux);

    /// <summary>Arrondi du net à payer — CDF à l'unité (franc entier), USD au centime.</summary>
    public static decimal RoundInvoicePayable(decimal amount, bool isUsd) =>
        isUsd
            ? Math.Round(amount, 2, MidpointRounding.AwayFromZero)
            : Math.Round(amount, 0, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Factures CDF : montants attendus en FC en base.
    /// Legacy : certaines lignes stockaient le prix USD (ex. 0,36) avec le libellé FC — on reconvertit.
    /// </summary>
    public static decimal NormalizeLegacyFactCdf(decimal amount, decimal taux)
    {
        if (taux <= 0 || amount <= 0) return amount;
        if (amount < 1000 && amount != Math.Round(amount, 0))
            return ToCdf(amount, true, taux);
        return amount;
    }

    public static string Cdf(decimal amount, string format = "N2") =>
        $"{amount.ToString(format, Fr)} FC";

    public static string Cdf(double amount, string format = "N2") =>
        Cdf((decimal)amount, format);

    public static string Usd(decimal amount, string format = "N2") =>
        $"$ {amount.ToString(format, Fr)}";

    public static string Usd(double amount, string format = "N2") =>
        Usd((decimal)amount, format);

    /// <summary>Affichage par défaut hors facturation — montant en USD.</summary>
    public static string Business(decimal amountUsd, string format = "N2") => Usd(amountUsd, format);

    public static string Business(double amountUsd, string format = "N2") => Business((decimal)amountUsd, format);

    /// <summary>Dashboard : USD converti depuis FC, avec décimales si montant &lt; 100 $.</summary>
    public static string BusinessDashboard(decimal amountUsd) =>
        Math.Abs(amountUsd) < 100 ? Usd(amountUsd, "N2") : Usd(amountUsd, "N0");

    /// <summary>Total facture stocké en FC → USD (utilise Total_Apres_Reduction si présent).</summary>
    public static decimal FactStoredTotalToUsd(decimal? totalApresReduction, decimal? total, decimal factTaux, decimal latestTaux)
    {
        var taux = factTaux > 0 ? factTaux : latestTaux;
        var amountFc = totalApresReduction ?? total ?? 0;
        return CdfStoredToUsd(amountFc, taux);
    }

    public static decimal FactStoredTotalToFc(decimal? totalApresReduction, decimal? total, decimal factTaux, decimal latestTaux)
    {
        var taux = factTaux > 0 ? factTaux : latestTaux;
        return NormalizeLegacyFactCdf(totalApresReduction ?? total ?? 0, taux);
    }

    /// <summary>Montant stocké en FC (factures) → USD pour affichage dashboard/rapports.</summary>
    public static decimal CdfStoredToUsd(decimal amountCdf, decimal taux) =>
        taux > 0 ? Math.Round(NormalizeLegacyFactCdf(amountCdf, taux) / taux, 2) : amountCdf;

    /// <summary>Alias explicite : FC stocké → USD affichage.</summary>
    public static decimal StorageFcToUsd(decimal fc, decimal taux) => CdfStoredToUsd(fc, taux);

    /// <summary>USD affichage/saisie → FC stocké.</summary>
    public static decimal StorageUsdToFc(decimal usd, decimal taux) => ToCdf(usd, true, taux);

    public static string BusinessFromCdf(decimal amountCdf, decimal taux, string format = "N2") =>
        Usd(CdfStoredToUsd(amountCdf, taux), format);

    /// <summary>Montant appro/stock stocké en FC — affichage natif FC (conversion USD via DisplayFromFcStored).</summary>
    public static string ApproNative(decimal amountFc, bool _, string format = "N2") =>
        Cdf(RoundFcUp(amountFc), format == "N2" ? "N0" : format);

    /// <summary>Stat dashboard : montant FC stocké → affichage selon préférence utilisateur.</summary>
    public static string DisplayDashboardFromFc(decimal amountFc, decimal taux, int preferredMoneyId)
    {
        var fc = NormalizeLegacyFactCdf(amountFc, taux);
        if (IsUsdMoneyId(preferredMoneyId))
        {
            var usd = CdfStoredToUsd(fc, taux);
            return BusinessDashboard(usd);
        }
        return Cdf(fc, "N0");
    }

    /// <summary>Quatuor standard à partir d'un montant FC stocké (devise de référence).</summary>
    public static MonetarySnapshot FromFcStorage(decimal fc, decimal taux)
    {
        taux = MonetaryStandard.ResolveTaux(taux);
        var fcNorm = RoundFcUp(NormalizeLegacyFactCdf(fc, taux));
        var usd = taux > 0
            ? Math.Round(fcNorm / taux, 2, MidpointRounding.AwayFromZero)
            : 0;
        return new MonetarySnapshot(fcNorm, CurrencyDefaults.MoneyIdCdf, taux, usd);
    }

    /// <summary>Prix article natif → affichage FC converti (USD × taux, arrondi par excès).</summary>
    public static string CatalogAsCdf(decimal amountNative, bool isUsd, decimal taux, string format = "N0") =>
        amountNative <= 0 ? "—" : Cdf(ArticleToFc(amountNative, isUsd, taux), format);

    /// <summary>Équivalent dans l'autre devise (ligne secondaire).</summary>
    public static string ApproEquiv(decimal amountNative, bool isUsd, decimal taux, string format = "N2") =>
        taux <= 0 ? "" :
        isUsd ? $"≈ {Cdf(ToCdf(amountNative, true, taux), format)}" :
                $"≈ {Usd(ToUsd(amountNative, false, taux), format)}";

    public static string ApproUsdColumn(decimal amountNative, bool isUsd, decimal taux, string format = "N2")
    {
        var (usd, _) = Split(amountNative, isUsd, taux);
        return usd == 0 && !isUsd ? "—" : Usd(usd, format);
    }

    public static string ApproFcColumn(decimal amountNative, bool isUsd, decimal taux, string format = "N0")
    {
        var fc = ArticleToFc(amountNative, isUsd, taux);
        return fc <= 0 ? "—" : Cdf(fc, format);
    }

    /// <summary>Montant déjà dans la devise d'affichage (FC ou USD converti).</summary>
    public static string Invoice(decimal amount, bool isUsd, string format = "N2") =>
        isUsd ? Usd(amount, format) : Cdf(amount, format);

    /// <summary>Montant stocké en FC, converti si facture USD.</summary>
    public static string Fact(decimal amountFc, bool isUsd, decimal taux, string format = "N2") =>
        Invoice(ToDisplayAmount(amountFc, isUsd, taux), isUsd, format);

    public static string Fact(double amountFc, bool isUsd, decimal taux, string format = "N2") =>
        Fact((decimal)amountFc, isUsd, taux, format);

    /// <summary>Montant facture + équivalent dans l'autre devise (taux requis).</summary>
    public static string FactWithEquiv(decimal amountFc, bool isUsd, decimal taux, string format = "N2")
    {
        var primary = Fact(amountFc, isUsd, taux, format);
        if (taux <= 0) return primary;
        var equiv = isUsd
            ? Cdf(NormalizeLegacyFactCdf(amountFc, taux), format)
            : Usd(CdfStoredToUsd(amountFc, taux), format);
        return $"{primary} ≈ {equiv}";
    }

    public static string FactWithEquiv(double amountFc, bool isUsd, decimal taux, string format = "N2") =>
        FactWithEquiv((decimal)amountFc, isUsd, taux, format);

    /// <summary>Quatuor monétaire standard : montant natif + devise + taux + équivalent converti.</summary>
    public readonly record struct MonetarySnapshot(
        decimal Montant,
        int MoneyId,
        decimal Taux,
        decimal MontantApresConversion);

    /// <summary>
    /// Calcule le snapshot monétaire d'une ligne (total = Pu × Qté).
    /// <paramref name="puStoredInCdf"/> : true si Pu est en FC (caisse magasin), false si Pu est dans la devise facture (prestations).
    /// </summary>
    public static MonetarySnapshot ComputeMonetarySnapshot(
        decimal pu, decimal qte, int moneyId, decimal taux, bool puStoredInCdf = true)
    {
        var isUsd = IsUsdMoneyId(moneyId);
        var raw = pu * qte;
        decimal montant;
        decimal montantApres;

        if (puStoredInCdf)
        {
            var fc = NormalizeLegacyFactCdf(raw, taux);
            if (isUsd)
            {
                montant = taux > 0 ? Math.Round(fc / taux, 2, MidpointRounding.AwayFromZero) : 0;
                montantApres = Math.Round(fc, 0, MidpointRounding.AwayFromZero);
            }
            else
            {
                montant = Math.Round(fc, 0, MidpointRounding.AwayFromZero);
                montantApres = taux > 0 ? Math.Round(fc / taux, 2, MidpointRounding.AwayFromZero) : 0;
            }
        }
        else if (isUsd)
        {
            montant = Math.Round(raw, 2, MidpointRounding.AwayFromZero);
            montantApres = taux > 0 ? RoundFcUp(raw * taux) : 0;
            if (montantApres <= 0 && raw > 0)
                montantApres = 1;
        }
        else
        {
            montant = RoundFcUp(raw);
            if (montant <= 0 && raw > 0)
                montant = 1;
            montantApres = taux > 0 ? Math.Round(montant / taux, 2, MidpointRounding.AwayFromZero) : 0;
        }

        return new MonetarySnapshot(montant, moneyId, taux, montantApres);
    }

    public static bool IsUsdMoneyId(int moneyId) => moneyId == CurrencyDefaults.MoneyIdUsd;

    /// <summary>Affiche un montant dans la devise choisie par l'utilisateur.</summary>
    public static string DisplayForPreference(
        decimal nativeAmount, int nativeMoneyId, decimal taux, int preferredMoneyId, string format = "N2")
    {
        if (nativeMoneyId == preferredMoneyId)
            return IsUsdMoneyId(preferredMoneyId)
                ? Usd(nativeAmount, format)
                : Cdf(RoundFcUp(nativeAmount), format);

        var isNativeUsd = IsUsdMoneyId(nativeMoneyId);
        var (usd, fc) = Split(nativeAmount, isNativeUsd, taux);
        return IsUsdMoneyId(preferredMoneyId) ? Usd(usd, format) : Cdf(fc, format);
    }

    /// <summary>Affiche depuis le quatuor monétaire selon la préférence utilisateur.</summary>
    public static string DisplayFromSnapshot(
        decimal? montant, int? moneyId, decimal? taux, decimal? montantApresConversion,
        decimal fallbackAmount, int fallbackMoneyId, decimal fallbackTaux,
        int preferredMoneyId, string format = "N2")
    {
        if (montant.HasValue && moneyId.HasValue && taux.HasValue && montantApresConversion.HasValue)
        {
            if (moneyId.Value == preferredMoneyId)
                return IsUsdMoneyId(preferredMoneyId)
                    ? Usd(montant.Value, format)
                    : Cdf(montant.Value, format);
            return IsUsdMoneyId(preferredMoneyId)
                ? Usd(montantApresConversion.Value, format)
                : Cdf(montantApresConversion.Value, format);
        }

        return DisplayForPreference(fallbackAmount, fallbackMoneyId, fallbackTaux, preferredMoneyId, format);
    }

    /// <summary>
    /// Montant affichable dans la devise choisie — lit le quatuor BDD sans double conversion.
    /// </summary>
    public static decimal ResolveDisplayAmount(
        decimal? montant, int? moneyId, decimal? taux, decimal? montantApresConversion,
        decimal fallbackAmount, int fallbackMoneyId, decimal fallbackTaux,
        int preferredMoneyId)
    {
        if (montant.HasValue && moneyId.HasValue && taux is > 0 && montantApresConversion.HasValue)
        {
            return moneyId.Value == preferredMoneyId
                ? montant.Value
                : montantApresConversion.Value;
        }

        var isNativeUsd = IsUsdMoneyId(fallbackMoneyId);
        if (fallbackMoneyId == preferredMoneyId)
        {
            return isNativeUsd
                ? Math.Round(fallbackAmount, 2, MidpointRounding.AwayFromZero)
                : Math.Round(NormalizeLegacyFactCdf(fallbackAmount, fallbackTaux), 0, MidpointRounding.AwayFromZero);
        }

        var (usd, fc) = Split(fallbackAmount, isNativeUsd, fallbackTaux);
        return IsUsdMoneyId(preferredMoneyId) ? usd : fc;
    }

    /// <summary>PU cohérent : total affiché ÷ quantité (évite PU×Qté ≠ total).</summary>
    public static decimal ResolveUnitPriceFromLineTotal(decimal lineTotalDisplay, double qte, int preferredMoneyId)
    {
        if (qte <= 0) return lineTotalDisplay;
        var unit = lineTotalDisplay / (decimal)qte;
        if (IsUsdMoneyId(preferredMoneyId))
            return Math.Round(unit, 2, MidpointRounding.AwayFromZero);
        return Math.Abs(qte % 1) < 0.0001
            ? Math.Round(unit, 0, MidpointRounding.AwayFromZero)
            : Math.Round(unit, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Montant facture stocké en FC → affichage selon préférence utilisateur.</summary>
    public static string DisplayFromFcStored(decimal amountFc, decimal taux, int preferredMoneyId, string format = "N2")
    {
        if (IsUsdMoneyId(preferredMoneyId))
            return BusinessFromCdf(amountFc, taux, format);
        return Cdf(NormalizeLegacyFactCdf(amountFc, taux), format == "N2" ? "N0" : format);
    }

    /// <summary>Stat dashboard : valeur agrégée en USD → affichage selon préférence.</summary>
    public static string DisplayDashboard(decimal amountUsd, decimal taux, int preferredMoneyId)
    {
        if (IsUsdMoneyId(preferredMoneyId))
            return BusinessDashboard(amountUsd);
        var fc = taux > 0
            ? RoundFcUp(amountUsd * taux)
            : amountUsd;
        if (fc <= 0 && amountUsd > 0)
            fc = 1;
        return Cdf(fc, "N0");
    }

    /// <summary>Montant déjà exprimé dans la devise de l'opération.</summary>
    public static MonetarySnapshot FromNativeAmount(decimal amountNative, int moneyId, decimal taux)
    {
        var isUsd = IsUsdMoneyId(moneyId);
        var montant = isUsd
            ? Math.Round(amountNative, 2, MidpointRounding.AwayFromZero)
            : RoundFcUp(amountNative);
        if (!isUsd && montant <= 0 && amountNative > 0)
            montant = 1;

        var apres = isUsd
            ? (taux > 0 ? RoundFcUp(montant * taux) : 0)
            : (taux > 0 ? Math.Round(montant / taux, 2, MidpointRounding.AwayFromZero) : 0);
        if (isUsd && apres <= 0 && amountNative > 0)
            apres = 1;
        return new MonetarySnapshot(montant, moneyId, taux, apres);
    }

    /// <summary>Total facture stocké en FC (legacy) → snapshot standard.</summary>
    public static MonetarySnapshot FromFactStoredTotal(decimal totalFc, int moneyId, decimal taux)
    {
        var isUsd = IsUsdMoneyId(moneyId);
        var fc = NormalizeLegacyFactCdf(totalFc, taux);
        var montant = ToDisplayAmount(totalFc, isUsd, taux);
        var apres = isUsd ? Math.Round(fc, 0, MidpointRounding.AwayFromZero) : CdfStoredToUsd(totalFc, taux);
        return new MonetarySnapshot(montant, moneyId, taux, apres);
    }

    /// <summary>Applique le snapshot sur un enregistrement monétaire.</summary>
    public static void ApplySnapshot(IMonetaryRecord record, MonetarySnapshot snapshot)
    {
        record.Montant = snapshot.Montant;
        record.MoneyId = snapshot.MoneyId;
        record.Taux = snapshot.Taux;
        record.MontantApresConversion = snapshot.MontantApresConversion;
    }

    /// <summary>Lecture snapshot : stocké en base ou recalculé (legacy).</summary>
    public static MonetarySnapshot ResolveMonetarySnapshot(
        decimal? montant, int? moneyId, decimal? taux, decimal? montantApresConversion,
        decimal pu, decimal qte, int fallbackMoneyId, decimal fallbackTaux, bool puStoredInCdf = true)
    {
        if (montant.HasValue && moneyId.HasValue && taux.HasValue && montantApresConversion.HasValue)
            return new MonetarySnapshot(montant.Value, moneyId.Value, taux.Value, montantApresConversion.Value);

        return ComputeMonetarySnapshot(pu, qte, fallbackMoneyId, fallbackTaux, puStoredInCdf);
    }

    /// <summary>Montant principal + équivalent (ligne facture, vendeur).</summary>
    public static string FactLineDualDisplay(
        decimal? montant, int? moneyId, decimal? taux, decimal? montantApresConversion,
        decimal pu, decimal qte, int fallbackMoneyId, decimal fallbackTaux, bool puStoredInCdf = true,
        string primaryFormat = "N2", string equivFormat = "N0")
    {
        var parts = FactLineDualParts(montant, moneyId, taux, montantApresConversion,
            pu, qte, fallbackMoneyId, fallbackTaux, puStoredInCdf, primaryFormat, equivFormat);
        return $"{parts.Primary} · {parts.Equivalent}";
    }

    /// <summary>Bloc HTML-friendly : montant natif + conversion en dessous.</summary>
    public static (string Primary, string Equivalent) FactLineDualParts(
        decimal? montant, int? moneyId, decimal? lineTaux, decimal? montantApresConversion,
        decimal pu, decimal qte, int fallbackMoneyId, decimal fallbackTaux, bool puStoredInCdf = true,
        string primaryFormat = "N2", string equivFormat = "N0")
    {
        var snap = ResolveMonetarySnapshot(montant, moneyId, lineTaux, montantApresConversion,
            pu, qte, fallbackMoneyId, fallbackTaux, puStoredInCdf);
        var isUsd = IsUsdMoneyId(snap.MoneyId);
        var primary = isUsd ? Usd(snap.Montant, primaryFormat) : Cdf(snap.Montant, equivFormat);
        var equiv = isUsd ? Cdf(snap.MontantApresConversion, equivFormat) : Usd(snap.MontantApresConversion, primaryFormat);
        return (primary, equiv);
    }
}
