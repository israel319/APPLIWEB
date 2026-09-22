using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Shared;

[Table("T_Status_Fact_Details")]
public class StatusFactDetail
{
    [Key]
    [Column("Id_Status")]
    public int IdStatus { get; set; }

    [MaxLength(50)]
    [Column("Description_Status")]
    public string? DescriptionStatus { get; set; }
}
