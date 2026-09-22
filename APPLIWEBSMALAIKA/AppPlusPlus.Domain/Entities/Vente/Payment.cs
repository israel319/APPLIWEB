using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Domain.Entities.Vente;

/// <summary>
/// Correspond a la table [dbo].[T_Payments] -- Paiements sur factures.
/// </summary>
[Table("T_Payments")]
public class Payment : IMonetaryRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("Id_Fact")]
    public int IdFact { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Montant { get; set; }

    [Column("Id_Monais")]
    public int? MoneyId { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? Taux { get; set; }

    [Column("Montant_Apres_Conversion", TypeName = "decimal(18,2)")]
    public decimal? MontantApresConversion { get; set; }

    [MaxLength(200)]
    public string? Note { get; set; }

    [Required]
    public DateTime DateSys { get; set; } = DateTime.Now;

    [MaxLength(50)]
    [Column("User")]
    public string? User { get; set; }

    // Navigation
    [ForeignKey(nameof(IdFact))]
    public Fact? Fact { get; set; }

    [ForeignKey(nameof(MoneyId))]
    public Money? Money { get; set; }

    decimal? IMonetaryRecord.Montant
    {
        get => Montant;
        set => Montant = value ?? 0;
    }
}
