using AppPlusPlus.Domain.Entities.Approvisionnement;

namespace AppPlusPlus.Domain.Common;

/// <summary>Filtres cohérents pour rapports, tableaux de bord et KPI approvisionnement (montants FC stockés).</summary>
public static class ApproFilters
{
    public static bool IsCancelled(Appro a) => a.StatusId == ApproStatus.Cancelled;

    public static bool IsInternalTransfer(Appro a) => a.StatusId == ApproStatus.Transfer;

    /// <summary>Approvisionnement externe comptabilisé (achat réel, hors transfert interne et annulé).</summary>
    public static bool IsCountedPurchase(Appro a) =>
        !IsCancelled(a) && !IsInternalTransfer(a);

    public static IEnumerable<Appro> CountedPurchases(IEnumerable<Appro> appros) =>
        appros.Where(IsCountedPurchase);

    /// <summary>Montant d'achat stocké en FC (PA × Qté).</summary>
    public static decimal PurchaseAmountFc(Appro a) => (a.PA ?? 0) * a.Qte;

    public static decimal TotalPurchaseFc(IEnumerable<Appro> appros) =>
        CountedPurchases(appros).Sum(PurchaseAmountFc);

    public static decimal TotalBenefitFc(IEnumerable<Appro> appros) =>
        CountedPurchases(appros).Sum(a => a.BenTotal ?? 0);

    [Obsolete("Utiliser PurchaseAmountFc — montants stockés en FC.")]
    public static decimal PurchaseAmountUsd(Appro a) => PurchaseAmountFc(a);

    [Obsolete("Utiliser TotalPurchaseFc — montants stockés en FC.")]
    public static decimal TotalPurchaseUsd(IEnumerable<Appro> appros) => TotalPurchaseFc(appros);

    [Obsolete("Utiliser TotalBenefitFc — montants stockés en FC.")]
    public static decimal TotalBenefitUsd(IEnumerable<Appro> appros) => TotalBenefitFc(appros);
}
