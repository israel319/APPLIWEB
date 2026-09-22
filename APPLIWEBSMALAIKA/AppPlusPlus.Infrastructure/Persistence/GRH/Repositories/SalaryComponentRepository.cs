using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Payroll;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class SalaryComponentRepository : ISalaryComponentRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public SalaryComponentRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<SalaryComponent> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.SalaryComponents.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new InvalidOperationException($"Composante salariale ID {id} introuvable.");
    }

    public async Task<IEnumerable<SalaryComponent>> GetAllActiveAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.SalaryComponents
            .Where(s => s.IsActive)
            .OrderBy(s => s.ComponentType).ThenBy(s => s.ComponentName)
            .ToListAsync();
    }

    public async Task AddAsync(SalaryComponent component)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.SalaryComponents.Add(component);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(SalaryComponent component)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.SalaryComponents.Update(component);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var c = await ctx.SalaryComponents.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new InvalidOperationException($"Composante salariale ID {id} introuvable.");
        ctx.SalaryComponents.Remove(c);
        await ctx.SaveChangesAsync();
    }
}
