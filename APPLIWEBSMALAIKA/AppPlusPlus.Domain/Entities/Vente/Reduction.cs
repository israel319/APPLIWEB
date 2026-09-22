using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Vente;

[Table("T_Reductions")]
public class Reduction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id_Reduction")]
    public int IdReduction { get; set; }

    [MaxLength(50)]
    [Column("Description_Reduction")]
    public string? DescriptionReduction { get; set; }

    public int? Valeur { get; set; }
}
