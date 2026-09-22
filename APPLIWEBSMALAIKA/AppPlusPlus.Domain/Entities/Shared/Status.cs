using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Shared;

[Table("T_Status")]
public class Status
{
    [Key]
    [Column("Status")]
    public int StatusId { get; set; }

    [MaxLength(50)]
    [Column("Status_")]
    public string? StatusDescription { get; set; }
}
