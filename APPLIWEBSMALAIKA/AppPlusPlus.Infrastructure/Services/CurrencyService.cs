using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Application.Services.Shared;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Finance;

namespace AppPlusPlus.Infrastructure.Services;

public class CurrencyService : ICurrencyService
{
    private readonly IFinanceRepository _financeRepo;
    private readonly IUserCurrencyPreferenceService _userCurrency;

    public CurrencyService(IFinanceRepository financeRepo, IUserCurrencyPreferenceService userCurrency)
    {
        _financeRepo = financeRepo;
        _userCurrency = userCurrency;
    }

    public async Task<decimal> GetLatestTauxAsync(CancellationToken cancellationToken = default)
    {
        var entity = await GetLatestTauxEntityAsync(cancellationToken);
        return ResolveTaux(entity?.TauxValue);
    }

    public async Task<Taux?> GetLatestTauxEntityAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await _financeRepo.GetCurrentTauxAsync();
    }

    public decimal ResolveTaux(decimal? taux) => MonetaryStandard.ResolveTaux(taux);

    public decimal NormalizeStorageFc(decimal fc) => CurrencyFormat.RoundFcUp(fc);

    public decimal FcToUsd(decimal fc, decimal? taux = null) =>
        CurrencyFormat.CdfStoredToUsd(NormalizeStorageFc(fc), ResolveTaux(taux));

    public decimal UsdToFc(decimal usd, decimal? taux = null) =>
        CurrencyFormat.ToCdf(usd, true, ResolveTaux(taux));

    public CurrencyFormat.MonetarySnapshot FromFcStorage(decimal fc, decimal? taux = null) =>
        CurrencyFormat.FromFcStorage(NormalizeStorageFc(fc), ResolveTaux(taux));

    public void ApplyFcStorage(IMonetaryRecord record, decimal fc, decimal? taux = null) =>
        CurrencyFormat.ApplySnapshot(record, FromFcStorage(fc, taux));

    public string FormatStoredFc(decimal fc, decimal? taux = null, string format = "N2")
    {
        var rate = ResolveTaux(taux ?? _userCurrency.CachedTaux);
        return CurrencyFormat.DisplayFromFcStored(NormalizeStorageFc(fc), rate, _userCurrency.PreferredMoneyId, format);
    }

    public string FormatStoredFcDashboard(decimal fc, decimal? taux = null)
    {
        var rate = ResolveTaux(taux ?? _userCurrency.CachedTaux);
        return CurrencyFormat.DisplayDashboardFromFc(NormalizeStorageFc(fc), rate, _userCurrency.PreferredMoneyId);
    }

    public decimal? NormalizeApproPriceFromStorage(decimal? stored, decimal? rowTaux = null, int? moneyId = null)
    {
        if (!stored.HasValue || stored.Value <= 0)
            return stored;

        var taux = ResolveTaux(rowTaux ?? _userCurrency.CachedTaux);

        if (moneyId.HasValue)
        {
            if (CurrencyDefaults.IsUsdMoneyId(moneyId.Value))
                return NormalizeStorageFc(CurrencyFormat.ToCdf(stored.Value, true, taux));
            return NormalizeStorageFc(stored.Value);
        }

        // Legacy sans Id_Monais (très anciennes lignes)
        if (stored.Value < 500m && taux > 0)
            return NormalizeStorageFc(CurrencyFormat.ToCdf(stored.Value, true, taux));

        return NormalizeStorageFc(stored.Value);
    }
}
