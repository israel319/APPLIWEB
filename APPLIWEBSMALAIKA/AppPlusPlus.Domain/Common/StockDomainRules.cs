using AppPlusPlus.Domain.Entities.Stock;

namespace AppPlusPlus.Domain.Common;

/// <summary>Règles métier stock centralisées — une seule source pour tous les modules.</summary>
public static class StockDomainRules
{
    public static decimal TotalQuantity(IEnumerable<Stock> stocks) =>
        stocks.Sum(s => s.Qte);

    public static int TotalBudgetQuantity(IEnumerable<Stock> stocks) =>
        stocks.Sum(s => s.QteMax);

    public static int RemainingBudgetQuantity(IEnumerable<Stock> stocks) =>
        stocks.Sum(s => Math.Max(0, s.QteMax - (int)Math.Ceiling(s.Qte)));

    public static bool IsOutOfStock(IEnumerable<Stock> stocks) =>
        TotalQuantity(stocks) <= 0m;

    public static bool IsLowStock(IEnumerable<Stock> stocks)
    {
        var total = TotalQuantity(stocks);
        if (total <= 0m) return false;
        return total <= stocks.Sum(s => s.Seuil);
    }

    public static decimal AvailableAtLocation(Stock? stock) =>
        stock?.Qte ?? 0m;

    /// <summary>Quantité transférable = min(stock physique, restant logique commande).</summary>
    public static decimal TransferableQuantity(decimal stockAtLocation, decimal commandRemaining) =>
        Math.Max(0, Math.Min(stockAtLocation, commandRemaining));
}
