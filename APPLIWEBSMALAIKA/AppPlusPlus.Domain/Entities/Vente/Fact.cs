using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Clients;
using AppPlusPlus.Domain.Entities.Commandes;
using AppPlusPlus.Domain.Entities.Prestations;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Domain.Entities.Vente;

/// <summary>
/// Correspond a la table [dbo].[T_Facts] -- Factures.
/// </summary>
[Table("T_Facts")]
public class Fact : IMonetaryRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Column("Description_Name")]
    public string DescriptionName { get; set; } = string.Empty;

    [Column("Description_Article")]
    public string? DescriptionArticle { get; set; }

    [MaxLength(50)]
    public string? Adresse { get; set; }

    [MaxLength(50)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(13)]
    public string? Telephone { get; set; }

    public int? Type { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Reduction { get; set; }

    [Column("Total_Apres_Reduction", TypeName = "decimal(18,2)")]
    public decimal? TotalApresReduction { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Total { get; set; }

    [Column("Total_Reduit", TypeName = "decimal(18,2)")]
    public decimal? TotalReduit { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,0)")]
    public decimal Taux { get; set; }

    [Required]
    public int Status { get; set; }

    [Required]
    public DateTime DateSys { get; set; }

    [Required, MaxLength(50)]
    [Column("User")]
    public string User { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Column("Cumputer")]
    public string Cumputer { get; set; } = string.Empty;

    /// <summary>FK vers T_Customer. NULL = anciennes factures, sinon toujours renseigne.</summary>
    public int? CustomerId { get; set; }

    /// <summary>FK vers T_Commande. NULL = facture directe (sans commande).</summary>
    public int? CommandeId { get; set; }

    /// <summary>FK vers T_Livraison. Facture generee au paiement d'une livraison.</summary>
    public int? LivraisonId { get; set; }

    /// <summary>FK vers T_Service_Projects — facture generee depuis une prestation.</summary>
    [Column("Id_Service_Project")]
    public int? ServiceProjectId { get; set; }

    /// <summary>Montant net a payer en toutes lettres.</summary>
    [MaxLength(500)]
    public string? MontantEnLettres { get; set; }

    /// <summary>FK vers T_Moneys -- devise de la facture.</summary>
    [Column("Id_Monais")]
    public int? MoneyId { get; set; }

    /// <summary>Net à payer dans la devise de la facture (figé à la validation).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Montant { get; set; }

    /// <summary>Équivalent dans l'autre devise.</summary>
    [Column("Montant_Apres_Conversion", TypeName = "decimal(18,2)")]
    public decimal? MontantApresConversion { get; set; }

    // Navigation
    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    [ForeignKey(nameof(CommandeId))]
    public Commande? Commande { get; set; }

    [ForeignKey(nameof(LivraisonId))]
    public Livraison? Livraison { get; set; }

    [ForeignKey(nameof(ServiceProjectId))]
    public ServiceProject? ServiceProject { get; set; }

    [ForeignKey(nameof(MoneyId))]
    public Money? Money { get; set; }

    decimal? IMonetaryRecord.Taux
    {
        get => Taux;
        set { if (value.HasValue) Taux = value.Value; }
    }

    public ICollection<FactDetail> Details { get; set; } = new List<FactDetail>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
