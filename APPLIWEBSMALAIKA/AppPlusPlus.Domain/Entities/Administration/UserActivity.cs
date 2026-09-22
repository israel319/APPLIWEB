using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Administration;

[Table("T_User_Activities")]
public class UserActivity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserActivityId { get; set; }

    [Required, MaxLength(50)]
    public string UserLogin { get; set; } = string.Empty;

    [Required]
    public int ActivityId { get; set; }

    public bool IsGranted { get; set; } = true;

    public DateTime AssignedDate { get; set; } = DateTime.Now;

    [ForeignKey(nameof(UserLogin))]
    public User? User { get; set; }

    [ForeignKey(nameof(ActivityId))]
    public Activity? Activity { get; set; }
}
