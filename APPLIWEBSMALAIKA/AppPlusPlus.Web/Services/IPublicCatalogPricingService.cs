using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Web.Services;

/// <summary>
/// Taux et tarification catalogue unifiés pour toute la partie publique (catalogue, panier, commande).
/// </summary>
public interface IPublicCatalogPricingService
{
    decimal Taux { get; }

    bool IsLoaded { get; }

    Task EnsureLoadedAsync();

    CommandePricing.LineQuote QuoteLine(decimal nativePu, int nativeMoneyId, decimal qty);

    string FormatUnit(decimal nativePu, int nativeMoneyId, string format = "N0");

    string FormatLineTotal(decimal nativePu, int nativeMoneyId, decimal qty, string format = "N0");

    string FormatOrderTotal(
        IEnumerable<(decimal nativePu, int nativeMoneyId, decimal qty)> lines,
        string format = "N0");
}
