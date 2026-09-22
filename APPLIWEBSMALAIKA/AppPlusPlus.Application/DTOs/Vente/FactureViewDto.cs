using AppPlusPlus.Domain.Entities.Vente;

namespace AppPlusPlus.Application.DTOs.Vente;

/// <summary>
/// Enriched facture view used in the Paiements tab (VenteHub tab 2).
/// </summary>
public class FactureViewDto
{
    public Fact Facture { get; set; } = null!;
    public string ClientName { get; set; } = "";
    public decimal TotalPaye { get; set; }
    public decimal Reste { get; set; }
    /// <summary>Facture POS sans commande liée.</summary>
    public bool IsDirectSale { get; set; }
    public List<Payment> Payments { get; set; } = new();
}
