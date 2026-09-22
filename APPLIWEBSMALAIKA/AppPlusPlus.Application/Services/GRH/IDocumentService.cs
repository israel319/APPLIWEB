using AppPlusPlus.Domain.Entities.GRH.Documents;

namespace AppPlusPlus.Application.Services.GRH;

/// <summary>
/// Service pour gérer les documents
/// </summary>
public interface IDocumentService
{
    Task<EmployeeDocument> UploadDocumentAsync(int employeeId, int documentTypeId, string documentPath, DateTime? expiryDate);
    Task<EmployeeDocument> GetDocumentAsync(int id);
    Task<IEnumerable<EmployeeDocument>> GetEmployeeDocumentsAsync(int employeeId);
    Task<IEnumerable<EmployeeDocument>> GetExpiringDocumentsAsync(int daysUntilExpiry);
    Task<EmployeeAttestation> IssueAttestationAsync(int employeeId, string attestationType);
    Task<IEnumerable<EmployeeAttestation>> GetAttestationsAsync(int employeeId);
    Task UpdateDocumentAsync(EmployeeDocument document);
    Task DeleteDocumentAsync(int id);
    Task RenewDocumentAsync(int documentId, DateTime newExpiryDate);
}
