using AppPlusPlus.Application.Interfaces;
using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Vente;

namespace AppPlusPlus.Application.Services.Shared;

public class UserCurrencyPreferenceService : IUserCurrencyPreferenceService
{
    private readonly IUserRepository _userRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IFinanceRepository _financeRepo;

    private int _preferredMoneyId = CurrencyDefaults.MoneyIdCdf;
    private decimal _cachedTaux = MonetaryStandard.DefaultTauxFallback;
    private bool _loaded;
    private string? _loadedForLogin;

    public UserCurrencyPreferenceService(
        IUserRepository userRepo,
        ICurrentUserService currentUser,
        IFinanceRepository financeRepo)
    {
        _userRepo = userRepo;
        _currentUser = currentUser;
        _financeRepo = financeRepo;
    }

    public int PreferredMoneyId => _preferredMoneyId;
    public bool IsUsdDisplay => _preferredMoneyId == CurrencyDefaults.MoneyIdUsd;
    public bool IsLoaded => _loaded;
    public decimal CachedTaux => _cachedTaux;
    public int DisplayRevision { get; private set; }

    public event Action? OnChanged;

    public async Task EnsureLoadedAsync()
    {
        var login = _currentUser.Login;
        if (string.IsNullOrWhiteSpace(login))
            return;

        if (_loaded && string.Equals(_loadedForLogin, login, StringComparison.OrdinalIgnoreCase))
            return;

        var pref = await _userRepo.GetPreferredMoneyIdAsync(login);
        _preferredMoneyId = pref ?? CurrencyDefaults.MoneyIdCdf;
        var taux = await _financeRepo.GetCurrentTauxAsync();
        _cachedTaux = taux?.TauxValue > 0 ? taux.TauxValue : MonetaryStandard.DefaultTauxFallback;
        _loaded = true;
        _loadedForLogin = login;
    }

    public Task SetPreferredMoneyIdAsync(int moneyId)
    {
        if (moneyId != CurrencyDefaults.MoneyIdUsd && moneyId != CurrencyDefaults.MoneyIdCdf)
            moneyId = CurrencyDefaults.MoneyIdCdf;

        var login = _currentUser.Login;
        if (string.IsNullOrWhiteSpace(login))
            return Task.CompletedTask;

        if (_preferredMoneyId == moneyId && _loaded)
            return Task.CompletedTask;

        _preferredMoneyId = moneyId;
        _loaded = true;
        _loadedForLogin = login;
        DisplayRevision++;
        OnChanged?.Invoke();

        _ = PersistPreferredMoneyIdAsync(login, moneyId);
        return Task.CompletedTask;
    }

    async Task PersistPreferredMoneyIdAsync(string login, int moneyId)
    {
        try
        {
            await _userRepo.SetPreferredMoneyIdAsync(login, moneyId);
        }
        catch
        {
            // Préférence déjà appliquée en mémoire ; la persistance sera retentée au prochain changement.
        }
    }

    public string FormatAmount(decimal nativeAmount, int nativeMoneyId, decimal taux, string format = "N2") =>
        CurrencyFormat.DisplayForPreference(nativeAmount, nativeMoneyId, taux, _preferredMoneyId, format);

    public string FormatFromSnapshot(
        decimal? montant, int? moneyId, decimal? taux, decimal? montantApresConversion,
        decimal fallbackAmount, int fallbackMoneyId, decimal fallbackTaux,
        string format = "N2") =>
        CurrencyFormat.DisplayFromSnapshot(
            montant, moneyId, taux, montantApresConversion,
            fallbackAmount, fallbackMoneyId, fallbackTaux, _preferredMoneyId, format);

    public string FormatFromFcStored(decimal amountFc, decimal taux, string format = "N2") =>
        CurrencyFormat.DisplayFromFcStored(amountFc, taux, _preferredMoneyId, format);

    public string FormatFromFcStored(decimal amountFc, string format = "N2") =>
        FormatFromFcStored(amountFc, _cachedTaux, format);

    public string FormatBusiness(decimal amountFc, decimal taux, string format = "N2") =>
        FormatFromFcStored(amountFc, taux, format);

    public string FormatBusiness(decimal amountFc, string format = "N2") =>
        FormatFromFcStored(amountFc, format);

    public string FormatBusiness(double amountFc, decimal taux, string format = "N2") =>
        FormatBusiness((decimal)amountFc, taux, format);

    public string FormatBusiness(double amountFc, string format = "N2") =>
        FormatBusiness((decimal)amountFc, format);

    public string FormatApproNative(decimal amountFc, bool _, decimal taux, string format = "N2") =>
        FormatFromFcStored(amountFc, taux, format);

    public string FormatApproNative(decimal amountFc, bool _, string format = "N2") =>
        FormatFromFcStored(amountFc, format);

    public string FormatDashboard(decimal amountFc, decimal taux) =>
        CurrencyFormat.DisplayDashboardFromFc(amountFc, taux, _preferredMoneyId);

    public string FormatDashboard(decimal amountFc) =>
        FormatDashboard(amountFc, _cachedTaux);

    public string FormatPayment(Payment payment, decimal fallbackTaux, string format = "N2") =>
        FormatFromSnapshot(
            payment.Montant, payment.MoneyId, payment.Taux, payment.MontantApresConversion,
            payment.Montant, payment.MoneyId ?? PreferredMoneyId, fallbackTaux, format);

    public decimal ResolveDisplayAmount(
        decimal? montant, int? moneyId, decimal? taux, decimal? montantApresConversion,
        decimal fallbackAmount, int fallbackMoneyId, decimal fallbackTaux) =>
        CurrencyFormat.ResolveDisplayAmount(
            montant, moneyId, taux, montantApresConversion,
            fallbackAmount, fallbackMoneyId, fallbackTaux, _preferredMoneyId);

    public decimal ResolveLineUnitPrice(decimal lineTotalDisplay, double qte) =>
        CurrencyFormat.ResolveUnitPriceFromLineTotal(lineTotalDisplay, qte, _preferredMoneyId);

    public string FormatResolvedAmount(decimal amount, bool isUnitPrice = false)
    {
        if (IsUsdDisplay)
            return CurrencyFormat.Usd(amount, isUnitPrice ? "N2" : "N2");
        var format = isUnitPrice && amount != Math.Round(amount, 0)
            ? "N2"
            : "N0";
        return CurrencyFormat.Cdf(CurrencyFormat.RoundFcUp(amount), format);
    }
}
