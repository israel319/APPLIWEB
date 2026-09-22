using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Administration;

[Table("T_Localisations")]
public class Localisation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id_Localisation")]
    public int IdLocalisation { get; set; }

    [MaxLength(50)]
    [Column("Description_Localisation")]
    public string? DescriptionLocalisation { get; set; }

    [Column("Is_Depot")]
    public bool IsDepot { get; set; }

    // Navigation vers les utilisateurs assignes (many-to-many via T_User_Localisations)
    public ICollection<UserLocalisation> UserLocalisations { get; set; } = new List<UserLocalisation>();
}
