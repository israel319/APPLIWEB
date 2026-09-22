using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Domain.Entities.Commandes;

[Table("T_Commande_Details")]
public class CommandeDetail : IMonetaryRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int? CommandeId { get; set; }

    [MaxLength(50)]
    public string? ArticleId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Qte { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? QtePrise { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? QteRest { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Pu { get; set; }

    public double? Taux { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Montant { get; set; }

    [Column("Id_Monais")]
    public int? MoneyId { get; set; }

    [Column("Montant_Apres_Conversion", TypeName = "decimal(18,2)")]
    public decimal? MontantApresConversion { get; set; }

    decimal? IMonetaryRecord.Taux
    {
        get => Taux.HasValue ? (decimal)Taux.Value : null;
        set => Taux = value.HasValue ? (double)value.Value : null;
    }

    public int? Status { get; set; }

    // Navigation
    [ForeignKey(nameof(CommandeId))]
    public Commande? Commande { get; set; }

    [ForeignKey(nameof(ArticleId))]
    public Article? Article { get; set; }
}
