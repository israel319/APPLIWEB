namespace AppPlusPlus.Domain.Entities.GRH.Training;

/// <summary>
/// Entité représentant une session de formation
/// </summary>
public class TrainingSession
{
    public int Id { get; set; }
    public int TrainingProgramId { get; set; }
    public string SessionCode { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string? Trainer { get; set; }
    public int? MaxParticipants { get; set; }
    public string? Status { get; set; } // Planned/InProgress/Completed/Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public TrainingProgram TrainingProgram { get; set; } = null!;
    public ICollection<EmployeeTraining> EmployeeTrainings { get; set; } = new List<EmployeeTraining>();
}
