namespace AppPlusPlus.Domain.Entities.GRH.Absence;

/// <summary>
/// Entité représentant une absence d'employé
/// </summary>
public class Absence
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int AbsenceTypeId { get; set; }
    public string AbsenceNumber { get; set; } = null!;
    public DateTime AbsenceDate { get; set; }
    public int Duration { get; set; } // En heures ou jours
    public string? Reason { get; set; }
    public string? Justification { get; set; }
    public string? JustificationDocument { get; set; }
    public string? Status { get; set; } // Pending/Approved/Rejected
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee.Employee Employee { get; set; } = null!;
    public AbsenceType AbsenceType { get; set; } = null!;
}
