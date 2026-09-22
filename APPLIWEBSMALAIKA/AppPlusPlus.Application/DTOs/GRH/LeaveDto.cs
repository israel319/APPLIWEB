namespace AppPlusPlus.Application.DTOs.GRH.Leave;

public class CreateLeaveRequestDto
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public int? ReplacementPersonId { get; set; }
    public bool IsRetroactive { get; set; }
}

public class UpdateLeaveRequestDto
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public int? ReplacementPersonId { get; set; }
}

public class ApproveLeaveRequestDto
{
    public int Id { get; set; }
    public string? ApprovalNotes { get; set; }
}

public class RejectLeaveRequestDto
{
    public int Id { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

public class LeaveRequestReadDto
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysDuration => (EndDate - StartDate).Days + 1;
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LeaveRequestListDto
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class LeaveSaldoReadDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }
    public int CarriedOverDays { get; set; }
}
