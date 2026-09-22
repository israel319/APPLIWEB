using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Administration;

[Table("T_Activities")]
public class Activity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ActivityId { get; set; }

    [Required]
    public int FonctionId { get; set; }

    [Required, MaxLength(80)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Column("Description_Activity")]
    public string DescriptionActivity { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(FonctionId))]
    public Fonction? Fonction { get; set; }

    public ICollection<UserActivity> UserActivities { get; set; } = new List<UserActivity>();
}
