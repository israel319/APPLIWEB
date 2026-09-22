using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Finance;

[Table("T_Users_Caisse")]
public class UserCaisse
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    [Column("User")]
    public string User { get; set; } = string.Empty;

    public int CaisseId { get; set; }

    [Column("Begin_Date")]
    public DateOnly? BeginDate { get; set; }

    [Column("End_Date")]
    public DateOnly? EndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxAmount { get; set; }

    public bool? Activate { get; set; }
}
