using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Finance;

namespace AppPlusPlus.Application.Services.Shared;

/// <summary>
/// Service central de gestion des devises.
/// Règle métier : seul le Franc Congolais (FC) est enregistré en base ;
/// le Dollar (USD) est une conversion d'affichage via le taux actif.
/// </summary>
public interface ICurrencyService
{
    /// <summary>Taux de change actif (dernier enregistrement).</summary>
    Task<decimal> GetLatestTauxAsync(CancellationToken cancellationToken = default);

    Task<Taux?> GetLatestTauxEntityAsync(CancellationToken cancellationToken = default);

    decimal ResolveTaux(decimal? taux);

    /// <summary>Normalise un montant FC pour stockage (entier, arrondi par excès).</summary>
    decimal NormalizeStorageFc(decimal fc);

    /// <summary>FC stocké → USD affichage : Montant FC / taux.</summary>
    decimal FcToUsd(decimal fc, decimal? taux = null);

    /// <summary>USD saisi à l'écran → FC stocké (conversion saisie uniquement).</summary>
    decimal UsdToFc(decimal usd, decimal? taux = null);

    /// <summary>Quatuor monétaire standard : Montant = FC, MontantApresConversion = USD.</summary>
    CurrencyFormat.MonetarySnapshot FromFcStorage(decimal fc, decimal? taux = null);

    void ApplyFcStorage(IMonetaryRecord record, decimal fc, decimal? taux = null);

    /// <summary>Affiche un montant FC stocké selon la préférence utilisateur (via taux actif si USD).</summary>
    string FormatStoredFc(decimal fc, decimal? taux = null, string format = "N2");

    string FormatStoredFcDashboard(decimal fc, decimal? taux = null);

    /// <summary>
    /// Lit un PA/PV appro depuis la BDD (legacy USD ou FC post-migration).
    /// </summary>
    decimal? NormalizeApproPriceFromStorage(decimal? stored, decimal? rowTaux = null, int? moneyId = null);
}
