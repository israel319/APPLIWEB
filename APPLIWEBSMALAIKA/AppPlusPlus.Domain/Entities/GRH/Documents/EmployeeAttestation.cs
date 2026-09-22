namespace AppPlusPlus.Domain.Entities.GRH.Documents;

/// <summary>
/// Entité représentant une attestation d'employé
/// </summary>
public class EmployeeAttestation
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string AttestationNumber { get; set; } = null!;
    public string AttestationType { get; set; } = null!; // Employment/Salary/Experience/LeaveBalance
    public DateTime AttestationDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? IssuedBy { get; set; }
    public string? Signature { get; set; }
    public string AttestationDocument { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee.Employee Employee { get; set; } = null!;
}
