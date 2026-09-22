using AppPlusPlus.Domain.Entities.Commandes;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface ICommandeRepository : IRepository<Commande>
{
    Task<Commande?> GetWithDetailsAsync(int commandeId);
    Task<Commande?> GetWithDetailsAndLivraisonsAsync(int commandeId);
    Task<List<Commande>> GetByCustomerAsync(int customerId);

    /// <summary>
    /// Returns a customer's commandes with Details.Article, Livraisons and Factures loaded,
    /// ordered by CreationDate descending.
    /// </summary>
    Task<List<Commande>> GetByCustomerWithFullDetailsAsync(int customerId);

    /// <summary>Commandes liées à un client (pour calcul fidélité).</summary>
    Task<List<Commande>> GetForLoyaltyCalculationAsync();
    Task<List<Commande>> GetByStatusAsync(int status);
    Task<List<Commande>> GetByUserAsync(string userLogin);
    Task<List<Commande>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<List<Livraison>> GetLivraisonsByCommandeAsync(int commandeId);
    Task<Livraison?> GetLivraisonWithDetailsAsync(int livraisonId);
    Task AddLivraisonAsync(Livraison livraison);

    /// <summary>
    /// Returns all commandes with Customer, Details, Livraisons, and Factures loaded,
    /// ordered by CreationDate descending.
    /// </summary>
    Task<List<Commande>> GetAllWithFullDetailsAsync();

    /// <summary>
    /// For the given CommandeDetail IDs, returns the sum of delivered quantities
    /// grouped by CommandeDetail ID (from LivraisonDetails).
    /// </summary>
    Task<Dictionary<int, decimal>> GetDeliveredQtyByDetailIdsAsync(List<int> commandeDetailIds);

    /// <summary>
    /// For the given Commande IDs, returns the total payment amount per commande
    /// (sum of Payment.Montant across all Facts linked to each Commande).
    /// </summary>
    Task<Dictionary<int, decimal>> GetPaidAmountByCommandeIdsAsync(List<int> commandeIds);

    /// <summary>
    /// Persists status and payment amount corrections for the given commandes.
    /// Only updates Status, MontantPaye, and MontantRest columns.
    /// </summary>
    Task UpdateCommandeStatusBatchAsync(List<Commande> commandes);

    /// <summary>
    /// Returns a commande with all navigation properties loaded for detail view:
    /// Customer, Details.Article, Livraisons.Details, Livraisons.Fact.Payments.
    /// </summary>
    Task<Commande?> GetWithAllNavigationsAsync(int commandeId);

    /// <summary>
    /// Saves a commande with its details. If CommandeId == 0, inserts a new record;
    /// otherwise updates the existing commande and replaces its details.
    /// </summary>
    Task SaveCommandeWithDetailsAsync(Commande commande, List<CommandeDetail> details);
}
