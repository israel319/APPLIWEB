using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Employee;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class EmployeeContractRepository : IEmployeeContractRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public EmployeeContractRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<IEnumerable<EmployeeContract>> GetByEmployeeAsync(int employeeId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeContracts
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
    }

    public async Task<EmployeeContract> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.EmployeeContracts.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException($"Contrat ID {id} introuvable.");
    }

    public async Task AddAsync(EmployeeContract contract)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.EmployeeContracts.Add(contract);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(EmployeeContract contract)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.EmployeeContracts.Update(contract);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var c = await ctx.EmployeeContracts.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException($"Contrat ID {id} introuvable.");
        ctx.EmployeeContracts.Remove(c);
        await ctx.SaveChangesAsync();
    }
}
