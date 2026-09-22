using AppPlusPlus.Domain.Entities.GRH.Documents;
using AppPlusPlus.Application.Repositories.GRH;

namespace AppPlusPlus.Application.Services.GRH;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<EmployeeDocument> UploadDocumentAsync(int employeeId, int documentTypeId, string documentPath, DateTime? expiryDate)
    {
        var document = new EmployeeDocument
        {
            EmployeeId = employeeId,
            DocumentTypeId = documentTypeId,
            DocumentNumber = $"DOC-{DateTime.UtcNow:yyyyMMddHHmmss}",
            DocumentPath = documentPath,
            UploadDate = DateTime.UtcNow,
            ExpiryDate = expiryDate,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        await _documentRepository.AddDocumentAsync(document);
        return document;
    }

    public async Task<EmployeeDocument> GetDocumentAsync(int id)
    {
        return await _documentRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<EmployeeDocument>> GetEmployeeDocumentsAsync(int employeeId)
    {
        return await _documentRepository.GetByEmployeeAsync(employeeId);
    }

    public async Task<IEnumerable<EmployeeDocument>> GetExpiringDocumentsAsync(int daysUntilExpiry)
    {
        return await _documentRepository.GetExpiringDocumentsAsync(daysUntilExpiry);
    }

    public async Task<EmployeeAttestation> IssueAttestationAsync(int employeeId, string attestationType)
    {
        var attestation = new EmployeeAttestation
        {
            EmployeeId = employeeId,
            AttestationNumber = $"ATT-{DateTime.UtcNow:yyyyMMddHHmmss}",
            AttestationType = attestationType,
            AttestationDate = DateTime.UtcNow,
            AttestationDocument = $"attestation-{employeeId}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf",
            CreatedAt = DateTime.UtcNow
        };

        await _documentRepository.AddAttestationAsync(attestation);
        return attestation;
    }

    public async Task<IEnumerable<EmployeeAttestation>> GetAttestationsAsync(int employeeId)
    {
        return await _documentRepository.GetAttestationsByEmployeeAsync(employeeId);
    }

    public async Task UpdateDocumentAsync(EmployeeDocument document)
    {
        document.UpdatedAt = DateTime.UtcNow;
        await _documentRepository.UpdateDocumentAsync(document);
    }

    public async Task DeleteDocumentAsync(int id)
    {
        await _documentRepository.DeleteDocumentAsync(id);
    }

    public async Task RenewDocumentAsync(int documentId, DateTime newExpiryDate)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        document.ExpiryDate = newExpiryDate;
        document.UpdatedAt = DateTime.UtcNow;
        await _documentRepository.UpdateDocumentAsync(document);
    }
}
