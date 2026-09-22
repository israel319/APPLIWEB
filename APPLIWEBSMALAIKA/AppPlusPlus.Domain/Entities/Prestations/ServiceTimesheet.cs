using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Enums;

namespace AppPlusPlus.Domain.Entities.Prestations;

[Table("T_Service_Timesheets")]
public class ServiceTimesheet
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public int? TaskId { get; set; }

    [Required, MaxLength(50)]
    public string UserLogin { get; set; } = string.Empty;

    [Required]
    public DateOnly WorkDate { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal Hours { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int Status { get; set; } = (int)ServiceTimesheetStatus.Draft;

    public int? FactId { get; set; }

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
