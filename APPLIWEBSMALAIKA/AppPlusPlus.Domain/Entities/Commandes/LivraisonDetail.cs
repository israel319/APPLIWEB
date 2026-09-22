using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Domain.Entities.Commandes;

[Table("T_Livraison_Detail")]
public class LivraisonDetail : IMonetaryRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int? LivraisonId { get; set; }

    public int? CommandDetailId { get; set; }

    [MaxLength(100)]
    public string? ArticleId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Qte { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontantPaye { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Montant { get; set; }

    [Column("Id_Monais")]
    public int? MoneyId { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? Taux { get; set; }

    [Column("Montant_Apres_Conversion", TypeName = "decimal(18,2)")]
    public decimal? MontantApresConversion { get; set; }

    [MaxLength(50)]
    public string? Comment { get; set; }

    public DateTime? Date { get; set; }

    [MaxLength(50)]
    public string? UserLogin { get; set; }

    public DateTime? CreationDate { get; set; }

    [Column("Localisationid")]
    public int? LocalisationId { get; set; }

    // Navigation
    [ForeignKey(nameof(LivraisonId))]
    public Livraison? Livraison { get; set; }

    [ForeignKey(nameof(CommandDetailId))]
    public CommandeDetail? CommandeDetail { get; set; }

    [ForeignKey(nameof(ArticleId))]
    public Article? Article { get; set; }
}
