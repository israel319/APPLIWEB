using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Fournisseurs;

[Table("T_Supplier_Service")]
public class SupplierService
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ServiceId { get; set; }

    [MaxLength(50)]
    public string? ServiceDescription { get; set; }
}
