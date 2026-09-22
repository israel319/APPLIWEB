using AppPlusPlus.Domain.Enums.GRH;

namespace AppPlusPlus.Domain.Entities.GRH.Leave;

/// <summary>
/// Entité représentant une demande de congé
/// </summary>
public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public string RequestNumber { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfDays { get; set; }
    public string? Reason { get; set; }
    public string? ReplacementPersonCode { get; set; }
    public LeaveStatusEnum Status { get; set; } = LeaveStatusEnum.Draft;
    public string? Comments { get; set; }
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsRetroactive { get; set; }
    public bool IsPaid { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee.Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}
