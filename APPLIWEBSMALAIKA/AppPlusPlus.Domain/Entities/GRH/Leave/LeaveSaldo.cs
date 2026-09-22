namespace AppPlusPlus.Domain.Entities.GRH.Leave;

/// <summary>
/// Entité représentant le solde de congés d'un employé
/// </summary>
public class LeaveSaldo
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public int Year { get; set; }
    public decimal TotalEntitledDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public decimal CarriedOverDays { get; set; }
    public decimal CarriedOverExpiringDays { get; set; } // Jours reportés qui expire bientôt
    public DateTime? LastResetDate { get; set; }
    public bool HasExceededBalance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee.Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}
