using AppPlusPlus.Application.DTOs.Inventaire;

namespace AppPlusPlus.Application.Services.Inventaire;

public interface IInventaireService
{
    Task<List<InventaireSessionDto>> GetSessionsAsync(IReadOnlyList<int> locIds, bool hasGlobalScope);
    Task<InventaireSessionDto?> GetSessionAsync(int id);
    Task<InventaireSessionDto?> GetOpenSessionForLocalisationAsync(int localisationId);
    Task<decimal> GetSuggestedMontantInitialAsync(int localisationId);
    Task<List<InventaireSessionDto>> GetSessionHistoryForLocalisationAsync(int localisationId);
    Task<int> CreateSessionAsync(InventaireSessionCreateRequest request);
    Task UpdateMontantInitialAsync(int inventaireId, decimal montantInitial);
    Task CloseSessionAsync(int inventaireId, string userLogin);
    Task<List<InventaireReceptionDto>> GetReceptionsAsync(int inventaireId);
    Task<List<InventaireReceptionDto>> GetReceptionHistoryForLocalisationAsync(int localisationId);
    Task<InventaireReceptionDto?> GetReceptionAsync(int receptionId);
    Task<InventaireReceptionDto?> GetReceptionForDayAsync(int inventaireId, DateOnly dateJour);
    Task<int> SaveReceptionAsync(InventaireReceptionSaveRequest request);
    Task CloseReceptionAsync(int receptionId, string userLogin);
    Task<List<InventaireVenteDto>> GetVentesAsync(int inventaireId);
    Task<InventaireVenteDto?> GetVenteForDayAsync(int inventaireId, DateOnly dateVente);
    Task SaveVenteAsync(InventaireVenteSaveRequest request);
    Task DeleteVenteAsync(int venteId, int inventaireId);
    Task CloseDayAsync(InventaireCloseDayRequest request);
}
