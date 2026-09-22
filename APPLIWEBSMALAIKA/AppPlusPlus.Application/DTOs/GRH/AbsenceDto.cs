namespace AppPlusPlus.Application.DTOs.GRH.Absence;

public class CreateAbsenceDto
{
    public int EmployeeId { get; set; }
    public int AbsenceTypeId { get; set; }
    public DateTime AbsenceDate { get; set; }
    public int DurationDays { get; set; }
    public string? Reason { get; set; }
    public string? JustificationDocument { get; set; }
    public bool IsPaid { get; set; }
}

public class ApproveAbsenceDto
{
    public int Id { get; set; }
    public string? ApprovalNotes { get; set; }
}

public class AbsenceReadDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string AbsenceTypeName { get; set; } = string.Empty;
    public DateTime AbsenceDate { get; set; }
    public int DurationDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
}
