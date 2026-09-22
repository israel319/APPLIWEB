using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Clients;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Domain.Entities.Commandes;

[Table("T_Commande")]
public class Commande : IMonetaryRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CommandeId { get; set; }

    public int? CustomerId { get; set; }

    [MaxLength(50)]
    public string? CommandeDescription { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontantTotal { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontantPaye { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontantRest { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Montant { get; set; }

    [Column("Id_Monais")]
    public int? MoneyId { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? Taux { get; set; }

    [Column("Montant_Apres_Conversion", TypeName = "decimal(18,2)")]
    public decimal? MontantApresConversion { get; set; }

    public int? Status { get; set; }

    [MaxLength(50)]
    public string? UserLogin { get; set; }

    public DateTime? CreationDate { get; set; }

    // Navigation
    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    public ICollection<CommandeDetail> Details { get; set; } = new List<CommandeDetail>();

    /// <summary>Livraisons liees a cette commande.</summary>
    public ICollection<Livraison> Livraisons { get; set; } = new List<Livraison>();

    /// <summary>Factures liees (via CommandeId sur T_Facts, pour reference directe).</summary>
    public ICollection<Fact> Factures { get; set; } = new List<Fact>();
}
