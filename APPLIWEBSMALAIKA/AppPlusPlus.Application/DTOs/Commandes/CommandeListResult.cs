using AppPlusPlus.Domain.Entities.Commandes;

namespace AppPlusPlus.Application.DTOs.Commandes;

/// <summary>
/// Result returned by the commande service for the CommandeHub grid.
/// Contains the list of commandes (with nav properties loaded) plus
/// a lookup of delivered quantities per CommandeDetail Id.
/// </summary>
public class CommandeListResult
{
    public List<Commande> Commandes { get; set; } = new();

    /// <summary>
    /// Key = CommandeDetail.Id, Value = total delivered quantity across all livraisons.
    /// </summary>
    public Dictionary<int, decimal> DeliveredQtyByDetailId { get; set; } = new();
}
