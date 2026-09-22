namespace AppPlusPlus.Domain.Entities.GRH.Documents;

/// <summary>
/// Entité représentant un document d'employé
/// </summary>
public class EmployeeDocument
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int DocumentTypeId { get; set; }
    public string DocumentNumber { get; set; } = null!;
    public string? Description { get; set; }
    public string DocumentPath { get; set; } = null!;
    public DateTime UploadDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpiringSoon { get; set; }
    public string? Status { get; set; } // Active/Expired/Archived
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee.Employee Employee { get; set; } = null!;
    public DocumentType DocumentType { get; set; } = null!;
}
