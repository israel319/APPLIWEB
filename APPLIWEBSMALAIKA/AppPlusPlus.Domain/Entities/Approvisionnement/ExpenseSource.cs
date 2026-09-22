using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Approvisionnement;

[Table("T_Expense_Source")]
public class ExpenseSource
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    public string? Sources { get; set; }
}
