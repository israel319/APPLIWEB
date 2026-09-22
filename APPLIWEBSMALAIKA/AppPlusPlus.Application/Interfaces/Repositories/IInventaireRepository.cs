using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Domain.Entities.Inventaire;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface IInventaireRepository
{
    Task<List<InventaireMagasin>> GetSessionsForLocalisationsAsync(IReadOnlyList<int> locIds, bool hasGlobalScope);
    Task<InventaireMagasin?> GetSessionByIdAsync(int id);
    Task<InventaireMagasin?> GetOpenSessionForLocalisationAsync(int localisationId);
    Task<InventaireMagasin?> GetLastClosedSessionForLocalisationAsync(int localisationId);
    Task<List<InventaireMagasin>> GetClosedSessionsForLocalisationAsync(int localisationId, int take = 200);
    Task<InventaireReception?> GetReceptionByIdWithDetailsAsync(int receptionId);
    Task<InventaireReception?> GetReceptionForDayAsync(int inventaireId, DateOnly dateJour);
    Task<List<InventaireReception>> GetReceptionsForSessionAsync(int inventaireId);
    Task<List<InventaireReception>> GetReceptionHistoryForLocalisationAsync(int localisationId, int take = 500);
    Task<List<InventaireVente>> GetVentesForSessionAsync(int inventaireId);
    Task<InventaireVente?> GetVenteForDayAsync(int inventaireId, DateOnly dateVente);
    Task AddSessionAsync(InventaireMagasin session);
    Task UpdateSessionAsync(InventaireMagasin session);
    Task AddReceptionAsync(InventaireReception reception);
    Task UpdateReceptionAsync(InventaireReception reception);
    Task SaveReceptionDetailsAsync(int receptionId, List<InventaireReceptionDetail> details);
    Task UpsertVenteAsync(InventaireVente vente);
    Task DeleteVenteAsync(int venteId);
}
