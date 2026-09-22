using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Vente;

[Table("T_Moneys")]
public class Money
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id_Monais")]
    public int IdMonais { get; set; }

    [MaxLength(50)]
    [Column("Description_Monais")]
    public string? DescriptionMonais { get; set; }
}
