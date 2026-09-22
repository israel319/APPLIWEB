using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Administration;

[Table("T_Fonctions")]
public class Fonction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id_Fonction")]
    public int IdFonction { get; set; }

    [MaxLength(50)]
    [Column("Description_Fonction")]
    public string? DescriptionFonction { get; set; }

    // Navigation
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
