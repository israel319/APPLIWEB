using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Clients;
using AppPlusPlus.Domain.Entities.Vente;

namespace AppPlusPlus.Domain.Entities.Commandes;

[Table("T_Livraison")]
public class Livraison
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LivraisonId { get; set; }

    [MaxLength(50)]
    public string? Porteur { get; set; }

    /// <summary>FK vers T_Customer (colonne ClientId en BD).</summary>
    public int? ClientId { get; set; }

    /// <summary>FK vers T_Commande.</summary>
    public int? CommandeId { get; set; }

    public int? Status { get; set; }

    public DateTime? Date { get; set; }

    public DateTime? DateSys { get; set; }

    [MaxLength(50)]
    public string? UserLogin { get; set; }

    // Navigation
    [ForeignKey(nameof(ClientId))]
    public Customer? Customer { get; set; }

    [ForeignKey(nameof(CommandeId))]
    public Commande? Commande { get; set; }

    public ICollection<LivraisonDetail> Details { get; set; } = new List<LivraisonDetail>();

    /// <summary>Facture generee au paiement de cette livraison (0 ou 1).</summary>
    public Fact? Fact { get; set; }
}
