using AppPlusPlus.Domain.Entities.Commandes;

namespace AppPlusPlus.Domain.Common;

/// <summary>
/// Tarification commande alignée sur le catalogue (PU FC arrondi par excès × quantité).
/// Une seule règle pour affichage panier, enregistrement et totaux.
/// </summary>
public static class CommandePricing
{
    public readonly record struct LineQuote(decimal PuFc, decimal LineFc);

    public static LineQuote QuoteLine(decimal nativePu, int nativeMoneyId, decimal qty, decimal taux)
    {
        taux = MonetaryStandard.ResolveTaux(taux);
        var isUsd = CurrencyDefaults.IsUsdMoneyId(nativeMoneyId);
        var puFc = CurrencyFormat.ArticleToFc(nativePu, isUsd, taux);
        var lineFc = CurrencyFormat.ArticleLineToFc(nativePu, qty, isUsd, taux);
        return new LineQuote(puFc, lineFc);
    }

    public static decimal OrderTotalFc(
        IEnumerable<(decimal nativePu, int nativeMoneyId, decimal qty)> lines,
        decimal taux)
    {
        taux = MonetaryStandard.ResolveTaux(taux);
        return lines.Sum(l => QuoteLine(l.nativePu, l.nativeMoneyId, l.qty, taux).LineFc);
    }

    /// <summary>
    /// Quote ligne commande : priorité au prix natif catalogue, sinon PU stocké (déjà FC si MoneyId CDF).
    /// </summary>
    public static LineQuote QuoteDetailLine(
        decimal qty,
        decimal? storedPu,
        int? storedMoneyId,
        decimal? catalogNativePu,
        int? catalogMoneyId,
        decimal taux)
    {
        taux = MonetaryStandard.ResolveTaux(taux);
        if (catalogNativePu.HasValue && catalogMoneyId.HasValue)
            return QuoteLine(catalogNativePu.Value, catalogMoneyId.Value, qty, taux);

        var pu = storedPu ?? 0;
        var moneyId = storedMoneyId ?? CurrencyDefaults.MoneyIdUsd;
        if (CurrencyDefaults.IsCdfMoneyId(moneyId))
            return new LineQuote(pu, pu * qty);

        return QuoteLine(pu, moneyId, qty, taux);
    }

    public static decimal ResolveCommandeTotalFc(Commande commande)
    {
        var taux = MonetaryStandard.ResolveTaux(commande.Taux);
        return commande.Details.Sum(d => QuoteDetailLine(
            d.Qte ?? 0,
            d.Pu,
            d.MoneyId,
            d.Article != null ? (decimal?)d.Article.Price : null,
            d.Article?.IdMonais,
            taux).LineFc);
    }

    /// <summary>
    /// Applique la tarification catalogue : PU/total en FC, quatuor monétaire cohérent.
    /// Entrée : Pu = prix natif article, MoneyId = Id_Monais article.
    /// </summary>
    public static void ApplyCatalogPricing(
        Commande commande,
        IEnumerable<CommandeDetail> details,
        decimal taux,
        int defaultNativeMoneyId = CurrencyDefaults.MoneyIdCdf)
    {
        taux = MonetaryStandard.ResolveTaux(taux);
        decimal totalFc = 0;

        foreach (var d in details)
        {
            var nativeMoneyId = d.MoneyId ?? defaultNativeMoneyId;
            var quote = QuoteLine(d.Pu ?? 0, nativeMoneyId, d.Qte ?? 0, taux);

            d.Pu = quote.PuFc;
            d.MoneyId = CurrencyDefaults.MoneyIdCdf;
            d.Taux = (double?)taux;
            CurrencyFormat.ApplySnapshot(d,
                CurrencyFormat.FromNativeAmount(quote.LineFc, CurrencyDefaults.MoneyIdCdf, taux));
            totalFc += quote.LineFc;
        }

        var paid = commande.MontantPaye;
        commande.MontantTotal = totalFc;
        commande.MontantRest = Math.Max(0, totalFc - paid);
        commande.MoneyId = CurrencyDefaults.MoneyIdCdf;
        commande.Taux = taux;
        CurrencyFormat.ApplySnapshot(commande,
            CurrencyFormat.FromNativeAmount(totalFc, CurrencyDefaults.MoneyIdCdf, taux));
    }
}
