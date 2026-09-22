using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Administration;

[Table("T_User_Fonctions")]
public class UserFonction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    [Column("User")]
    public string? User { get; set; }

    [Column("Function")]
    public int? FunctionId { get; set; }

    [Column("Begin_Date")]
    public DateOnly? BeginDate { get; set; }

    [Column("End_Date")]
    public DateOnly? EndDate { get; set; }

    public bool? Activate { get; set; }
}
