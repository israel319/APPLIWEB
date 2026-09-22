namespace AppPlusPlus.Application.DTOs.GRH.Training;

public class CreateTrainingProgramDto
{
    public string ProgramCode { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public int DurationHours { get; set; }
    public decimal? Cost { get; set; }
    public bool IsInternal { get; set; }
}

public class CreateTrainingSessionDto
{
    public int TrainingProgramId { get; set; }
    public string SessionCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string? TrainerName { get; set; }
    public int MaxParticipants { get; set; }
}

public class EnrollEmployeeTrainingDto
{
    public int EmployeeId { get; set; }
    public int TrainingSessionId { get; set; }
    public string? Notes { get; set; }
}

public class TrainingProgramReadDto
{
    public int Id { get; set; }
    public string ProgramCode { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public int DurationHours { get; set; }
    public decimal? Cost { get; set; }
    public int SessionCount { get; set; }
    public int TotalEnrolledEmployees { get; set; }
}

public class TrainingSessionReadDto
{
    public int Id { get; set; }
    public string SessionCode { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string? TrainerName { get; set; }
    public int MaxParticipants { get; set; }
    public int EnrolledCount { get; set; }
    public string Status { get; set; } = string.Empty;
}
