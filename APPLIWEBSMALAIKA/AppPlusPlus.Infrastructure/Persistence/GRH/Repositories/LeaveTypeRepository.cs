using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Leave;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public LeaveTypeRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<LeaveType> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.LeaveTypes.FirstOrDefaultAsync(lt => lt.Id == id)
            ?? throw new InvalidOperationException($"Type de congé ID {id} introuvable.");
    }

    public async Task<IEnumerable<LeaveType>> GetAllActiveAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.LeaveTypes.Where(lt => lt.IsActive).OrderBy(lt => lt.LeaveTypeName).ToListAsync();
    }

    public async Task AddAsync(LeaveType leaveType)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.LeaveTypes.Add(leaveType);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(LeaveType leaveType)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.LeaveTypes.Update(leaveType);
        await ctx.SaveChangesAsync();
    }
}
