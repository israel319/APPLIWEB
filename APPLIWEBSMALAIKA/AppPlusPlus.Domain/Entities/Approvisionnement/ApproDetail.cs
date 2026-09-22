using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Domain.Entities.Approvisionnement;

/// <summary>
/// Lignes d'approvisionnement - details des articles recus
/// </summary>
[Table("T_Appro_Details")]
public class ApproDetail : IMonetaryRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Id_Appro")]
    public int IdAppro { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Id_Article")]
    public string IdArticle { get; set; } = string.Empty;

    [Column("Id_Localisation")]
    public int IdLocalisation { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Qte { get; set; }

    [Column("PA", TypeName = "decimal(18,2)")]
    public decimal? PA { get; set; }

    [Column("PV", TypeName = "decimal(18,2)")]
    public decimal? PV { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Ben { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BenTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Taux { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Montant { get; set; }

    [Column("Id_Monais")]
    public int? MoneyId { get; set; }

    [Column("Montant_Apres_Conversion", TypeName = "decimal(18,2)")]
    public decimal? MontantApresConversion { get; set; }

    public DateTime DateSys { get; set; } = DateTime.Now;

    [Column("Date_Expiration")]
    public DateOnly? DateExpiration { get; set; }

    [MaxLength(50)]
    [Column("User")]
    public string? User { get; set; }

    [MaxLength(50)]
    public string? Cumputer { get; set; }

    // Navigation properties
    [ForeignKey(nameof(IdAppro))]
    public Appro? Appro { get; set; }

    [ForeignKey(nameof(IdArticle))]
    public Article? Article { get; set; }

    [ForeignKey(nameof(IdLocalisation))]
    public Localisation? Localisation { get; set; }
}
