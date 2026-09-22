using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Documents;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public DocumentRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<EmployeeDocument> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeDocuments.FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new InvalidOperationException($"EmployeeDocument with ID {id} not found");
    }

    public async Task<EmployeeDocument?> GetByNumberAsync(string documentNumber)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeDocuments.FirstOrDefaultAsync(d => d.DocumentNumber == documentNumber);
    }

    public async Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(int employeeId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeDocuments.Where(d => d.EmployeeId == employeeId).ToListAsync();
    }

    public async Task<IEnumerable<EmployeeDocument>> GetExpiringDocumentsAsync(int daysUntilExpiry)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var expiryDate = DateTime.UtcNow.AddDays(daysUntilExpiry);
        return await ctx.EmployeeDocuments
            .Where(d => d.ExpiryDate <= expiryDate && d.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeAttestation>> GetAttestationsByEmployeeAsync(int employeeId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeAttestations.Where(a => a.EmployeeId == employeeId).ToListAsync();
    }

    public async Task AddDocumentAsync(EmployeeDocument document)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.EmployeeDocuments.Add(document);
        await ctx.SaveChangesAsync();
    }

    public async Task AddAttestationAsync(EmployeeAttestation attestation)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.EmployeeAttestations.Add(attestation);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateDocumentAsync(EmployeeDocument document)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.EmployeeDocuments.Update(document);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteDocumentAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var d = await ctx.EmployeeDocuments.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException($"EmployeeDocument with ID {id} not found");
        ctx.EmployeeDocuments.Remove(d);
        await ctx.SaveChangesAsync();
    }
}
