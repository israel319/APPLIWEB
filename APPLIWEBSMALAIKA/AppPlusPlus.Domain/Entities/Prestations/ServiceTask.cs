using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Enums;

namespace AppPlusPlus.Domain.Entities.Prestations;

[Table("T_Service_Tasks")]
public class ServiceTask
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ProjectId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? AssignedUser { get; set; }

    public DateOnly? PlannedStart { get; set; }
    public DateOnly? PlannedEnd { get; set; }

    public int Status { get; set; } = (int)ServiceTaskStatus.Pending;

    public int Priority { get; set; }

    [Required]
    public DateTime DateSys { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    [Column("User")]
    public string User { get; set; } = string.Empty;

    [ForeignKey(nameof(ProjectId))]
    public ServiceProject? Project { get; set; }

    public ICollection<ServiceTimesheet> Timesheets { get; set; } = new List<ServiceTimesheet>();
    public ICollection<ServiceDeliverable> Deliverables { get; set; } = new List<ServiceDeliverable>();
}
