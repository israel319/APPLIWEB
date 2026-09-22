using AppPlusPlus.Application.DTOs.Commandes;
using AppPlusPlus.Domain.Entities.Commandes;

namespace AppPlusPlus.Application.Services.Commandes;

public interface ICommandeService
{
    /// <summary>
    /// Returns all client commandes with full navigation properties loaded,
    /// auto-corrected statuses and payment amounts (persisted if changed),
    /// plus a delivered-quantity lookup.
    /// </summary>
    Task<CommandeListResult> GetCommandesWithStatusUpdateAsync();

    Task<List<Commande>> GetCommandesWithDetailsAsync();

    Task DeleteCommandeAsync(int commandeId);

    /// <summary>
    /// Determines whether the given commande still has at least one detail line
    /// where the ordered quantity exceeds the delivered quantity.
    /// </summary>
    bool HasRemainingToDeliver(Commande cmd, Dictionary<int, decimal> deliveredQty);

    /// <summary>
    /// Returns a single commande with its details loaded.
    /// </summary>
    Task<Commande?> GetCommandeWithDetailsAsync(int commandeId);

    /// <summary>
    /// Saves a commande with its details. If new (CommandeId == 0), inserts;
    /// otherwise updates and replaces detail lines.
    /// </summary>
    Task SaveCommandeAsync(Commande commande, List<CommandeDetail> details);

    /// <summary>
    /// Returns a fully composed detail view DTO for the CommandeDetailView dialog,
    /// including article rows with delivered quantities and livraison rows with payment status.
    /// </summary>
    Task<CommandeDetailViewDto?> GetCommandeDetailViewAsync(int commandeId);

    /// <summary>
    /// Returns the authenticated client's commandes with delivery progress and status corrections.
    /// </summary>
    Task<List<ClientCommandeSummaryDto>> GetClientCommandesAsync(int customerId);

    Task<Dictionary<int, CustomerLoyaltySummaryDto>> GetCustomerLoyaltySummariesAsync();

    Task<List<CustomerLoyaltyOrderLineDto>> GetCustomerLoyaltyOrdersAsync(int customerId);
}
