namespace AppPlusPlus.Domain.Entities.GRH.Leave;

/// <summary>
/// Entité représentant un type de congé
/// </summary>
public class LeaveType
{
    public int Id { get; set; }
    public string LeaveTypeCode { get; set; } = null!;
    public string LeaveTypeName { get; set; } = null!;
    public string? Description { get; set; }
    public int? DefaultDays { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public bool IsPaid { get; set; } = true;
    public bool IsRecurring { get; set; } = true; // Renouvelé annuellement
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveSaldo> LeaveSaldos { get; set; } = new List<LeaveSaldo>();
}
