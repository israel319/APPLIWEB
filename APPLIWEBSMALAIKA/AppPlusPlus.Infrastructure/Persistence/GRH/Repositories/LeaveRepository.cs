using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Leave;
using AppPlusPlus.Domain.Enums.GRH;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class LeaveRepository : ILeaveRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public LeaveRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<LeaveRequest> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new InvalidOperationException($"Leave request with ID {id} not found");
    }

    public async Task<LeaveRequest?> GetByNumberAsync(string requestNumber)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.LeaveRequests.FirstOrDefaultAsync(l => l.RequestNumber == requestNumber);
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.LeaveRequests.Where(l => l.EmployeeId == employeeId).ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.LeaveRequests.OrderByDescending(l => l.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.LeaveRequests.Where(l => l.Status == LeaveStatusEnum.Pending).ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.LeaveRequests.Where(l => l.Status.ToString() == status).ToListAsync();
    }

    public async Task AddAsync(LeaveRequest leaveRequest)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.LeaveRequests.Add(leaveRequest);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(LeaveRequest leaveRequest)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.LeaveRequests.Update(leaveRequest);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var lr = await ctx.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new InvalidOperationException($"Leave request with ID {id} not found");
        ctx.LeaveRequests.Remove(lr);
        await ctx.SaveChangesAsync();
    }

    public async Task ExecuteUpdateLeaveSaldoAsync(int employeeId, int year)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var conn = ctx.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_UpdateLeaveSaldo";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@EmployeeId", employeeId));
            cmd.Parameters.Add(new SqlParameter("@Year",       year));
            await cmd.ExecuteNonQueryAsync();
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
    }

    public async Task<(string Result, string Message)> ExecuteAnnualLeaveResetAsync(int year)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var conn = ctx.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_GenerateAnnualLeaveReset";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@CurrentYear", year));
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return (reader["Result"]?.ToString() ?? "ERROR",
                        reader["Message"]?.ToString() ?? "Unknown error");
            return ("ERROR", "No result from sp_GenerateAnnualLeaveReset");
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
    }
}
