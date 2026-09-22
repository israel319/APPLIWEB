using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Administration;

[Table("T_User_Localisations")]
public class UserLocalisation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Id_User")]
    [MaxLength(50)]
    public string? UserId { get; set; }

    [Column("Id_Localisation")]
    public int? LocalisationId { get; set; }

    [Column("Begin_Date")]
    public DateOnly? BeginDate { get; set; }

    [Column("End_Date")]
    public DateOnly? EndDate { get; set; }

    public bool? Activate { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(LocalisationId))]
    public Localisation? Localisation { get; set; }
}
