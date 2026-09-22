using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Finance;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Domain.Entities.Approvisionnement;

/// <summary>
/// T_Appro_Expense -- Versements / approvisionnement de caisse.
/// </summary>
[Table("T_Appro_Expense")]
public class ApproExpense : IMonetaryRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int SourceId { get; set; }

    public int? CaisseId { get; set; }

    public int? CurrencyId { get; set; }

    [Column("Id_Monais")]
    public int? MoneyId { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? Taux { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Montant { get; set; }

    [Column("Montant_Apres_Conversion", TypeName = "decimal(18,2)")]
    public decimal? MontantApresConversion { get; set; }

    /// <summary>FK vers T_Versements — si ce versement provient d'une clôture de journée.</summary>
    public int? VersementId { get; set; }

    [ForeignKey(nameof(VersementId))]
    public Versement? Versement { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AmountUSD { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AmountCDF { get; set; }

    [MaxLength(50)]
    public string? Depositeur { get; set; }

    [MaxLength(50)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Comment { get; set; }

    /// <summary>Statut du versement : En attente, Accepté, Rejeté.</summary>
    [MaxLength(20)]
    public string Statut { get; set; } = "En attente";

    [MaxLength(50)]
    public string? UserLogin { get; set; }

    public DateTime? CreationDate { get; set; }
}
