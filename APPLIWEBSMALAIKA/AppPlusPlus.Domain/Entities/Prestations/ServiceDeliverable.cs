using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Enums;

namespace AppPlusPlus.Domain.Entities.Prestations;

[Table("T_Service_Deliverables")]
public class ServiceDeliverable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public int? TaskId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateOnly? DueDate { get; set; }
    public DateOnly? DeliveredDate { get; set; }

    public int Status { get; set; } = (int)ServiceDeliverableStatus.Pending;

    [Required]
    public DateTime DateSys { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    [Column("User")]
    public string User { get; set; } = string.Empty;

    [ForeignKey(nameof(ProjectId))]
    public ServiceProject? Project { get; set; }

    [ForeignKey(nameof(TaskId))]
    public ServiceTask? Task { get; set; }
}
