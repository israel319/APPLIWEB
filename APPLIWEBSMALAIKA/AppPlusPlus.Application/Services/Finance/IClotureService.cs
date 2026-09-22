using AppPlusPlus.Application.DTOs.Finance;
using AppPlusPlus.Domain.Entities.Finance;

namespace AppPlusPlus.Application.Services.Finance;

public interface IClotureService
{
    /// <summary>
    /// Loads clotures (Versements) with their Localisation navigation,
    /// filtered by localisation IDs, ordered by date descending.
    /// </summary>
    Task<List<Versement>> GetCloturesByLocalisationsAsync(List<int> localisationIds);

    /// <summary>
    /// Returns true if a cloture (Versement) already exists for the given date and localisation.
    /// </summary>
    Task<bool> ClotureExistsAsync(DateOnly date, int localisationId);

    /// <summary>
    /// Aggregates paid factures and payments for the given date and localisation
    /// into a summary DTO for the cloture dialog.
    /// </summary>
    Task<ClotureSummaryDto> GetClotureSummaryAsync(DateOnly date, int localisationId);

    /// <summary>
    /// Persists a new cloture (Versement) record.
    /// </summary>
    Task CreateClotureAsync(Versement versement);

    /// <summary>
    /// True si l'utilisateur a une clôture du jour en attente d'approbation (statut 0).
    /// Après approbation (1) ou rejet (2), les transactions redeviennent possibles.
    /// </summary>
    Task<bool> HasUserClosedTodayAsync(string userLogin, List<int> localisationIds);

    /// <summary>
    /// Approves or rejects a closure. statut: 1 = approved, 2 = rejected. Returns false if not found.
    /// </summary>
    Task<bool> UpdateClotureStatutAsync(int versementId, int statut, string traitePar, string? motifRejet = null);

    /// <summary>Crée l'entrée caisse liée si absente (après approbation).</summary>
    Task EnsureClotureExpenseAsync(int versementId);

    /// <summary>Localisation avec le plus d'activité (ventes) pour la date.</summary>
    Task<int?> GetSuggestedLocalisationForClotureAsync(DateOnly date, List<int> localisationIds);

    /// <summary>Récapitulatif par localisation pour la même journée.</summary>
    Task<Dictionary<int, ClotureSummaryDto>> GetClotureSummariesAsync(DateOnly date, IEnumerable<int> localisationIds);
}
