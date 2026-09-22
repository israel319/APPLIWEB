using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class EmployeeCategoryRepository : IEmployeeCategoryRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public EmployeeCategoryRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<EmployeeCategory> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeCategories.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException($"EmployeeCategory with ID {id} not found");
    }

    public async Task<EmployeeCategory?> GetByCodeAsync(string categoryCode)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeCategories.FirstOrDefaultAsync(c => c.CategoryCode == categoryCode);
    }

    public async Task<IEnumerable<EmployeeCategory>> GetAllAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeCategories.ToListAsync();
    }

    public async Task AddAsync(EmployeeCategory category)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.EmployeeCategories.Add(category);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(EmployeeCategory category)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.EmployeeCategories.Update(category);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var cat = await ctx.EmployeeCategories.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException($"EmployeeCategory with ID {id} not found");
        ctx.EmployeeCategories.Remove(cat);
        await ctx.SaveChangesAsync();
    }
}
