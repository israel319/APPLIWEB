using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Catalogue;

namespace AppPlusPlus.Domain.Entities.Demandes;

[Table("T_Demande_Details")]
public class DemandeDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Id_Demande")]
    public int IdDemande { get; set; }

    [Column("Id_Article")]
    [MaxLength(100)]
    public string IdArticle { get; set; } = string.Empty;

    [Column("Qte_Demandee", TypeName = "decimal(18,2)")]
    public decimal QteDemandee { get; set; }

    [Column("Qte_Approuvee", TypeName = "decimal(18,2)")]
    public decimal? QteApprouvee { get; set; }

    [Column("Id_Stock_Demandeur")]
    public int? IdStockDemandeur { get; set; }

    [ForeignKey(nameof(IdDemande))]
    public Demande? Demande { get; set; }

    [ForeignKey(nameof(IdArticle))]
    public Article? Article { get; set; }
}
