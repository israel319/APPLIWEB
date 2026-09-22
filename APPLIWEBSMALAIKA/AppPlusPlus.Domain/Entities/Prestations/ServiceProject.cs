using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Clients;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Domain.Enums;

namespace AppPlusPlus.Domain.Entities.Prestations;

[Table("T_Service_Projects")]
public class ServiceProject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? CustomerId { get; set; }

    public int Status { get; set; } = (int)ServiceProjectStatus.Draft;

    /// <summary>0=Horaire, 1=Forfait, 2=Mixte</summary>
    public int BillingMode { get; set; } = (int)ServiceBillingMode.Hourly;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? HourlyRate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? FlatAmount { get; set; }

    public bool FlatInvoiced { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public int? LocalisationId { get; set; }

    public int? MoneyId { get; set; }

    [Required]
    public DateTime DateSys { get; set; } = DateTime.Now;

    [Required, MaxLength(50)]
    [Column("User")]
    public string User { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Cumputer { get; set; } = string.Empty;

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    [ForeignKey(nameof(MoneyId))]
    public Money? Money { get; set; }

    public ICollection<ServiceTask> Tasks { get; set; } = new List<ServiceTask>();
    public ICollection<ServiceTimesheet> Timesheets { get; set; } = new List<ServiceTimesheet>();
    public ICollection<ServiceDeliverable> Deliverables { get; set; } = new List<ServiceDeliverable>();
    public ICollection<Fact> Facts { get; set; } = new List<Fact>();
}
