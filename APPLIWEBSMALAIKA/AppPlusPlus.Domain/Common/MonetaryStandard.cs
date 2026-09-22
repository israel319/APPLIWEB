using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Domain.Entities.Commandes;
using AppPlusPlus.Domain.Entities.Finance;

namespace AppPlusPlus.Domain.Common;

/// <summary>
/// Application du quatuor monétaire standard à la sauvegarde (saisie métier).
/// </summary>
public static class MonetaryStandard
{
    public const decimal DefaultTauxFallback = 2800m;

    public static decimal ResolveTaux(decimal? taux) =>
        taux is > 0 ? taux.Value : DefaultTauxFallback;

    public static void ApplyCommande(Commande commande, IEnumerable<CommandeDetail> details, decimal taux,
        int moneyId = CurrencyDefaults.MoneyIdCdf)
    {
        CommandePricing.ApplyCatalogPricing(commande, details, taux, moneyId);
    }

    public static void ApplyLivraisonDetail(LivraisonDetail detail, decimal montantPaye, decimal taux,
        int moneyId = CurrencyDefaults.MoneyIdCdf)
    {
        taux = ResolveTaux(taux);
        detail.MontantPaye = montantPaye;
        detail.Taux = taux;
        CurrencyFormat.ApplySnapshot(detail,
            CurrencyFormat.FromFcStorage(montantPaye, taux));
    }

    public static void ApplyApproHeader(Appro appro, decimal montant, decimal taux,
        int moneyId = CurrencyDefaults.MoneyIdCdf)
    {
        taux = ResolveTaux(taux);
        appro.Taux = taux;
        CurrencyFormat.ApplySnapshot(appro,
            CurrencyFormat.FromFcStorage(montant, taux));
    }

    public static void ApplyApproDetail(ApproDetail detail, decimal lineMontant, decimal taux, int? moneyId = null)
    {
        taux = ResolveTaux(taux);
        detail.Taux = taux;
        CurrencyFormat.ApplySnapshot(detail,
            CurrencyFormat.FromFcStorage(lineMontant, taux));
    }

    public static void ApplyVersement(Versement versement, decimal taux,
        int moneyId = CurrencyDefaults.MoneyIdCdf)
    {
        taux = ResolveTaux(taux);
        CurrencyFormat.ApplySnapshot(versement,
            CurrencyFormat.FromFcStorage(versement.Montant, taux));
    }

    public static void ApplyApproExpense(ApproExpense expense, decimal taux)
    {
        taux = ResolveTaux(taux);

        decimal montant;
        int moneyId;

        if (expense.Montant is > 0 && expense.MoneyId.HasValue)
        {
            montant = expense.Montant.Value;
            moneyId = expense.MoneyId.Value;
        }
        else if (expense.CurrencyId.HasValue)
        {
            moneyId = expense.CurrencyId.Value;
            montant = CurrencyDefaults.IsCdfMoneyId(moneyId)
                ? expense.AmountCDF ?? 0
                : expense.AmountUSD ?? 0;
        }
        else if (expense.AmountCDF is > 0 && expense.AmountUSD is not > 0)
        {
            moneyId = CurrencyDefaults.MoneyIdCdf;
            montant = expense.AmountCDF.Value;
        }
        else if (expense.AmountUSD is > 0)
        {
            moneyId = CurrencyDefaults.MoneyIdUsd;
            montant = expense.AmountUSD.Value;
        }
        else
        {
            moneyId = expense.MoneyId ?? expense.CurrencyId ?? CurrencyDefaults.MoneyIdUsd;
            montant = expense.Montant ?? 0;
        }

        expense.Montant = montant;
        expense.MoneyId = moneyId;
        expense.Taux = taux;
        var fc = CurrencyDefaults.IsCdfMoneyId(moneyId)
            ? montant
            : CurrencyFormat.StorageUsdToFc(montant, taux);
        CurrencyFormat.ApplySnapshot(expense, CurrencyFormat.FromFcStorage(fc, taux));
    }
}
