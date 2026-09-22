using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Catalogue;

[Table("T_Mesures")]
public class Mesure
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id_Mesure")]
    public int IdMesure { get; set; }

    [MaxLength(50)]
    [Column("Description_Mesure")]
    public string? DescriptionMesure { get; set; }
}
