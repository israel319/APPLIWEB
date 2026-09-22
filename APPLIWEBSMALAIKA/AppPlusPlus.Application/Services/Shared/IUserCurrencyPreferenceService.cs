using AppPlusPlus.Domain.Entities.Vente;

namespace AppPlusPlus.Application.Services.Shared;

/// <summary>
/// Devise d'affichage et de saisie par défaut choisie par l'utilisateur (USD ou CDF).
/// </summary>
public interface IUserCurrencyPreferenceService
{
    int PreferredMoneyId { get; }
    bool IsUsdDisplay { get; }
    bool IsLoaded { get; }
    decimal CachedTaux { get; }

    /// <summary>Incrémenté à chaque changement — force le rafraîchissement des grilles MudBlazor.</summary>
    int DisplayRevision { get; }

    Task EnsureLoadedAsync();
    Task SetPreferredMoneyIdAsync(int moneyId);

    string FormatAmount(decimal nativeAmount, int nativeMoneyId, decimal taux, string format = "N2");

    string FormatFromSnapshot(
        decimal? montant, int? moneyId, decimal? taux, decimal? montantApresConversion,
        decimal fallbackAmount, int fallbackMoneyId, decimal fallbackTaux,
        string format = "N2");

    string FormatFromFcStored(decimal amountFc, decimal taux, string format = "N2");
    string FormatFromFcStored(decimal amountFc, string format = "N2");

    /// <summary>Montant FC stocké → affichage dashboard (conversion USD si préférence utilisateur).</summary>
    string FormatDashboard(decimal amountFc, decimal taux);
    string FormatDashboard(decimal amountFc);

    /// <summary>Montant FC stocké → affichage métier (conversion USD si préférence utilisateur).</summary>
    string FormatBusiness(decimal amountFc, decimal taux, string format = "N2");
    string FormatBusiness(decimal amountFc, string format = "N2");
    string FormatBusiness(double amountFc, decimal taux, string format = "N2");
    string FormatBusiness(double amountFc, string format = "N2");

    /// <summary>Montant FC stocké (appro/stock).</summary>
    string FormatApproNative(decimal amountFc, bool _, decimal taux, string format = "N2");
    string FormatApproNative(decimal amountFc, bool _, string format = "N2");

    string FormatPayment(Payment payment, decimal fallbackTaux, string format = "N2");

    decimal ResolveDisplayAmount(
        decimal? montant, int? moneyId, decimal? taux, decimal? montantApresConversion,
        decimal fallbackAmount, int fallbackMoneyId, decimal fallbackTaux);

    decimal ResolveLineUnitPrice(decimal lineTotalDisplay, double qte);

    string FormatResolvedAmount(decimal amount, bool isUnitPrice = false);

    event Action? OnChanged;
}
