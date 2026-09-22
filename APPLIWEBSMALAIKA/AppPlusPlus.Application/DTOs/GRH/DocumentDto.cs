namespace AppPlusPlus.Application.DTOs.GRH.Document;

public class UploadDocumentDto
{
    public int EmployeeId { get; set; }
    public int DocumentTypeId { get; set; }
    public string DocumentPath { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

public class IssueAttestationDto
{
    public int EmployeeId { get; set; }
    public string AttestationType { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public int? ValidityDays { get; set; }
    public string? IssuerName { get; set; }
    public string? Notes { get; set; }
}

public class DocumentReadDto
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpiringSoon { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AttestationReadDto
{
    public int Id { get; set; }
    public string AttestationNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string AttestationType { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? IssuerName { get; set; }
    public string Status { get; set; } = string.Empty;
}
