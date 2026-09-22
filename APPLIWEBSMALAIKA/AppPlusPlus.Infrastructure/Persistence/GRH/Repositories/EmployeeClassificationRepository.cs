using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class EmployeeClassificationRepository : IEmployeeClassificationRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public EmployeeClassificationRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<EmployeeClassification> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeClassifications.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException($"EmployeeClassification with ID {id} not found");
    }

    public async Task<EmployeeClassification?> GetByCodeAsync(string classificationCode)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeClassifications.FirstOrDefaultAsync(c => c.ClassificationCode == classificationCode);
    }

    public async Task<IEnumerable<EmployeeClassification>> GetAllAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeClassifications.ToListAsync();
    }

    public async Task AddAsync(EmployeeClassification classification)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.EmployeeClassifications.Add(classification);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(EmployeeClassification classification)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.EmployeeClassifications.Update(classification);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var cls = await ctx.EmployeeClassifications.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException($"EmployeeClassification with ID {id} not found");
        ctx.EmployeeClassifications.Remove(cls);
        await ctx.SaveChangesAsync();
    }
}
