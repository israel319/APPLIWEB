using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Payroll;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class PayrollRepository : IPayrollRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public PayrollRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<PayrollRun> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PayrollRuns.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new InvalidOperationException($"Payroll run with ID {id} not found");
    }

    public async Task<PayrollRun?> GetByNumberAsync(string payrollNumber)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PayrollRuns.FirstOrDefaultAsync(p => p.PayrollNumber == payrollNumber);
    }

    public async Task<IEnumerable<PayrollRun>> GetByYearAndMonthAsync(int year, int month)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PayrollRuns
            .Where(p => p.PayPeriodStart.Year == year && p.PayPeriodStart.Month == month)
            .OrderByDescending(p => p.PayPeriodStart)
            .ToListAsync();
    }

    public async Task<IEnumerable<PayrollRun>> GetByStatusAsync(string status)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PayrollRuns.Where(p => p.Status.ToString() == status).ToListAsync();
    }

    public async Task<IEnumerable<PayrollRun>> GetAllAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PayrollRuns.ToListAsync();
    }

    public async Task AddAsync(PayrollRun payrollRun)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.PayrollRuns.Add(payrollRun);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(PayrollRun payrollRun)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.PayrollRuns.Update(payrollRun);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var run = await ctx.PayrollRuns.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new InvalidOperationException($"Payroll run with ID {id} not found");
        ctx.PayrollRuns.Remove(run);
        await ctx.SaveChangesAsync();
    }

    public async Task<(string Result, string Message)> ExecuteCalculatePayrollAsync(
        int payrollRunId, DateTime periodStart, DateTime periodEnd, int calculatedBy)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var conn = ctx.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText   = "sp_CalculatePayroll";
            cmd.CommandType   = System.Data.CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add(new SqlParameter("@PayrollRunId",    payrollRunId));
            cmd.Parameters.Add(new SqlParameter("@PeriodStartDate", periodStart));
            cmd.Parameters.Add(new SqlParameter("@PeriodEndDate",   periodEnd));
            cmd.Parameters.Add(new SqlParameter("@CalculatedBy",    calculatedBy));

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return (reader["Result"]?.ToString() ?? "ERROR",
                        reader["Message"]?.ToString() ?? "Unknown error");

            return ("ERROR", "No result returned from sp_CalculatePayroll");
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
    }
}
