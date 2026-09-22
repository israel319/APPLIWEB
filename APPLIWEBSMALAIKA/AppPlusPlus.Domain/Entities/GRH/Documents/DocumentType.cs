namespace AppPlusPlus.Domain.Entities.GRH.Documents;

/// <summary>
/// Entité représentant un type de document
/// </summary>
public class DocumentType
{
    public int Id { get; set; }
    public string DocumentTypeCode { get; set; } = null!;
    public string DocumentTypeName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public int? RetentionDays { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public ICollection<EmployeeDocument> EmployeeDocuments { get; set; } = new List<EmployeeDocument>();
}
