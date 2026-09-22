using AppPlusPlus.Application.Common;
using AppPlusPlus.Domain.Entities.Commandes;

namespace AppPlusPlus.Application.Services.Commandes;

public interface ILivraisonService
{
    /// <summary>
    /// Loads livraisons with all navigations, filtered by localisations and user.
    /// </summary>
    Task<List<Livraison>> GetLivraisonsByLocalisationsAsync(List<int> localisationIds, string login);

    /// <summary>
    /// Deletes a livraison and its detail lines.
    /// </summary>
    Task DeleteLivraisonAsync(int livraisonId);

    /// <summary>
    /// Marks a livraison as delivered (status 1) and updates the commande status if applicable.
    /// </summary>
    Task MarkLivraisonDeliveredAsync(int livraisonId);

    /// <summary>
    /// Pays a livraison: creates an invoice + payment, updates livraison status to 2,
    /// and updates the related commande if applicable.
    /// Returns ServiceResult with the generated facture Id.
    /// </summary>
    Task<ServiceResult<int>> PayerLivraisonAsync(int livraisonId, string login);
}
