namespace AppPlusPlus.Domain.Entities.GRH.Employee;

/// <summary>
/// Entité représentant un contrat d'employé
/// </summary>
public class EmployeeContract
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string ContractNumber { get; set; } = null!;
    public string ContractType { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public string? Terms { get; set; }
    public string? DocumentPath { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee Employee { get; set; } = null!;
}
