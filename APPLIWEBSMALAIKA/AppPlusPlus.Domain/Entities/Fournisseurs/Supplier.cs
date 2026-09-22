using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Fournisseurs;

[Table("T_Supplier")]
public class Supplier
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SupplierId { get; set; }

    public int? ServiceId { get; set; }

    [MaxLength(10)]
    public string? SupplierNumber { get; set; }

    [MaxLength(50)]
    public string? SupplierName { get; set; }

    [MaxLength(50)]
    public string? Contact { get; set; }

    [MaxLength(50)]
    public string? Email { get; set; }

    public int? Currency { get; set; }

    [MaxLength(50)]
    public string? Adresse { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CcAmountToPay { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CcAmountPay { get; set; }

    [MaxLength(50)]
    public string? UserLogin { get; set; }

    public DateOnly? CreationDate { get; set; }
}
