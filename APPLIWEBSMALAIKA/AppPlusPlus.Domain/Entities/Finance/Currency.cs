using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Finance;

/// <summary>
/// T_Currency -- table lookup devises.
/// </summary>
[Table("T_Currency")]
public class Currency
{
    [Key]
    [Column("CurrencyId")]
    public int CurrencyId { get; set; }

    [Column("CurrenncyDescription")]
    public string? CurrencyDescription { get; set; }
}
