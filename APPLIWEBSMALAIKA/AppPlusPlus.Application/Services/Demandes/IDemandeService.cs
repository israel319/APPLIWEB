using AppPlusPlus.Application.DTOs.Demandes;

namespace AppPlusPlus.Application.Services.Demandes;

public interface IDemandeService
{
    Task<List<DemandeRowDto>> GetDemandesAsync(DemandeUserContext user);
    Task<List<DemandeDetailLineDto>> GetDetailLinesAsync(int demandeId);
    Task<DemandeActionResult> ApproveAdminAsync(
        DemandeUserContext user, int demandeId, Dictionary<int, decimal> qteParDetail, string? commentaire);
    Task<DemandeActionResult> RefuseAdminAsync(DemandeUserContext user, int demandeId, string? commentaire);
    Task<DemandeActionResult> ApproveAgentAsync(DemandeUserContext user, int demandeId);
    Task<DemandeActionResult> RefuseAgentAsync(DemandeUserContext user, int demandeId, string? commentaire);
}
