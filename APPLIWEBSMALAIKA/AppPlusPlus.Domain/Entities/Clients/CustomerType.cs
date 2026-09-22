using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Clients;

[Table("T_Customer_Type")]
public class CustomerType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CustomerTypeId")]
    public int CustomerTypeId { get; set; }

    [MaxLength(50)]
    [Column("Description")]
    public string? Description { get; set; }
}
