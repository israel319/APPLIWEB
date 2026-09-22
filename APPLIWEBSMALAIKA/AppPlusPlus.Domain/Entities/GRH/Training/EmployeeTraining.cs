namespace AppPlusPlus.Domain.Entities.GRH.Training;

/// <summary>
/// Entité représentant la formation d'un employé
/// </summary>
public class EmployeeTraining
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int TrainingSessionId { get; set; }
    public string? Status { get; set; } // Enrolled/InProgress/Completed/Cancelled
    public bool Attended { get; set; }
    public decimal? Score { get; set; }
    public bool Certified { get; set; }
    public DateTime? CertificateDate { get; set; }
    public string? Feedback { get; set; }
    public int? Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee.Employee Employee { get; set; } = null!;
    public TrainingSession TrainingSession { get; set; } = null!;
}
