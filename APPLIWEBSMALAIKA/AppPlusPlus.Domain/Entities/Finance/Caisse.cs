using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Finance;

[Table("T_Caisse")]
public class Caisse
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CaisseId { get; set; }

    [MaxLength(50)]
    public string Description { get; set; } = string.Empty;

    public int CurrencyId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DayLimit { get; set; }

    [MaxLength(50)]
    public string? UserLogin { get; set; }

    public DateTime? CreationDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Solde { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalDueDay { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalRestDay { get; set; }
}
