using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Payroll;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class PayrollDetailRepository : IPayrollDetailRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public PayrollDetailRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<IEnumerable<PayrollDetail>> GetByPayrollRunAsync(int payrollRunId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PayrollDetails.Where(d => d.PayrollRunId == payrollRunId).OrderBy(d => d.EmployeeId).ToListAsync();
    }

    public async Task<PayrollDetail?> GetByEmployeeAndRunAsync(int payrollRunId, int employeeId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PayrollDetails.FirstOrDefaultAsync(d => d.PayrollRunId == payrollRunId && d.EmployeeId == employeeId);
    }

    public async Task AddAsync(PayrollDetail detail)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.PayrollDetails.Add(detail);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(PayrollDetail detail)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.PayrollDetails.Update(detail);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteByRunAsync(int payrollRunId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var details = await ctx.PayrollDetails.Where(d => d.PayrollRunId == payrollRunId).ToListAsync();
        ctx.PayrollDetails.RemoveRange(details);
        await ctx.SaveChangesAsync();
    }
}
