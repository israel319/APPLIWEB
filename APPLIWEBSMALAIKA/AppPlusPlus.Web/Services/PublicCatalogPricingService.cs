using AppPlusPlus.Application.Services.Finance;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Web.Services;

public class PublicCatalogPricingService : IPublicCatalogPricingService
{
    readonly IFinanceService _financeService;
    decimal _taux = MonetaryStandard.DefaultTauxFallback;
    bool _loaded;

    public PublicCatalogPricingService(IFinanceService financeService) =>
        _financeService = financeService;

    public decimal Taux => _taux;

    public bool IsLoaded => _loaded;

    public async Task EnsureLoadedAsync()
    {
        if (_loaded)
            return;

        try
        {
            var tauxList = await _financeService.GetTauxAsync();
            var latest = tauxList.OrderByDescending(t => t.Id).FirstOrDefault()?.TauxValue;
            _taux = MonetaryStandard.ResolveTaux(latest);
        }
        catch
        {
            _taux = MonetaryStandard.DefaultTauxFallback;
        }

        _loaded = true;
    }

    public CommandePricing.LineQuote QuoteLine(decimal nativePu, int nativeMoneyId, decimal qty) =>
        CommandePricing.QuoteLine(nativePu, nativeMoneyId, qty, _taux);

    public string FormatUnit(decimal nativePu, int nativeMoneyId, string format = "N0") =>
        CurrencyFormat.CatalogAsCdf(nativePu, CurrencyDefaults.IsUsdMoneyId(nativeMoneyId), _taux, format);

    public string FormatLineTotal(decimal nativePu, int nativeMoneyId, decimal qty, string format = "N0") =>
        CurrencyFormat.Cdf(QuoteLine(nativePu, nativeMoneyId, qty).LineFc, format);

    public string FormatOrderTotal(
        IEnumerable<(decimal nativePu, int nativeMoneyId, decimal qty)> lines,
        string format = "N0") =>
        CurrencyFormat.Cdf(CommandePricing.OrderTotalFc(lines, _taux), format);
}
