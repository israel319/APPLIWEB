using AppPlusPlus.Application.Common;
using AppPlusPlus.Application.DTOs.Vente;
using AppPlusPlus.Domain.Entities.Vente;

namespace AppPlusPlus.Application.Services.Vente;

public interface IFacturationService
{
    /// <summary>
    /// Loads facture rows for the grid, filtered by localisations and excluding cloture dates.
    /// </summary>
    Task<List<FactRowDto>> GetFactureRowsAsync(List<int> localisationIds, string login);

    /// <summary>
    /// Deletes a facture and all its detail lines.
    /// </summary>
    Task<ServiceResult> DeleteFactureAsync(int factId);

    /// <summary>
    /// Loads enriched facture views for the Paiements tab (status 1-2 only).
    /// </summary>
    Task<List<FactureViewDto>> GetPaiementsAsync(List<int> localisationIds, string login);

    /// <summary>
    /// Loads a single facture with its details and their articles (for detail/print views).
    /// </summary>
    Task<Fact?> GetFactureWithDetailsAsync(int factId);

    /// <summary>
    /// Total encaissé aujourd'hui par l'opérateur (somme des paiements du jour, convertie en USD).
    /// </summary>
    Task<OperatorDailySalesDto> GetOperatorDailySalesAsync(string login, DateOnly date);

    /// <summary>
    /// Totaux encaissés hier et aujourd'hui par l'opérateur.
    /// </summary>
    Task<OperatorSalesSummaryDto> GetOperatorSalesSummaryAsync(string login);
}
