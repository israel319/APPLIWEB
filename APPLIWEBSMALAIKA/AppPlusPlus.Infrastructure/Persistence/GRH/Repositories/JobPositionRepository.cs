using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class JobPositionRepository : IJobPositionRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public JobPositionRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<JobPosition> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.JobPositions.Include(j => j.Department).Include(j => j.Employees)
            .FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new InvalidOperationException($"JobPosition with ID {id} not found");
    }

    public async Task<JobPosition?> GetByCodeAsync(string positionCode)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.JobPositions.FirstOrDefaultAsync(j => j.PositionCode == positionCode);
    }

    public async Task<IEnumerable<JobPosition>> GetByDepartmentAsync(int departmentId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.JobPositions.Include(j => j.Department).Include(j => j.Employees)
            .Where(j => j.DepartmentId == departmentId).OrderBy(j => j.PositionName).ToListAsync();
    }

    public async Task<IEnumerable<JobPosition>> GetAllAsync()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.JobPositions.Include(j => j.Department).Include(j => j.Employees)
            .OrderBy(j => j.PositionName).ToListAsync();
    }

    public async Task AddAsync(JobPosition jobPosition)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.JobPositions.Add(jobPosition);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(JobPosition jobPosition)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.JobPositions.Update(jobPosition);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var pos = await ctx.JobPositions.FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new InvalidOperationException($"JobPosition with ID {id} not found");
        ctx.JobPositions.Remove(pos);
        await ctx.SaveChangesAsync();
    }
}
