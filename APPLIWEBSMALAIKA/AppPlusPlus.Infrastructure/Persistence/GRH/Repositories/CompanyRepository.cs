using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public CompanyRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<Company> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Companies.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException($"Company with ID {id} not found");
    }

    public async Task<Company?> GetByCodeAsync(string companyCode)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Companies.FirstOrDefaultAsync(c => c.CompanyCode == companyCode);
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Companies.Where(c => c.IsActive).OrderBy(c => c.CompanyName).ToListAsync();
    }

    public async Task AddAsync(Company company)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Companies.Add(company);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Company company)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.Companies.Update(company);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var company = await ctx.Companies.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException($"Company with ID {id} not found");
        ctx.Companies.Remove(company);
        await ctx.SaveChangesAsync();
    }
}
