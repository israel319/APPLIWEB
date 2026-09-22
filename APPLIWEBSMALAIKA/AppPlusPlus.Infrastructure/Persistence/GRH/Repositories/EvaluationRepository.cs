using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Evaluation;
using Microsoft.EntityFrameworkCore;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Repositories;

public class EvaluationRepository : IEvaluationRepository
{
    private readonly IDbContextFactory<GrhDbContext> _contextFactory;

    public EvaluationRepository(IDbContextFactory<GrhDbContext> contextFactory)
        => _contextFactory = contextFactory;

    public async Task<PerformanceEvaluation> GetByIdAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PerformanceEvaluations.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new InvalidOperationException($"PerformanceEvaluation with ID {id} not found");
    }

    public async Task<PerformanceEvaluation?> GetByCodeAsync(string evaluationCode)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PerformanceEvaluations.FirstOrDefaultAsync(e => e.EvaluationCode == evaluationCode);
    }

    public async Task<IEnumerable<PerformanceEvaluation>> GetByEmployeeAsync(int employeeId)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PerformanceEvaluations.Where(e => e.EmployeeId == employeeId).ToListAsync();
    }

    public async Task<IEnumerable<PerformanceEvaluation>> GetByYearAsync(int year)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PerformanceEvaluations.Where(e => e.Year == year).ToListAsync();
    }

    public async Task<IEnumerable<PerformanceEvaluation>> GetByStatusAsync(string status)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        return await ctx.PerformanceEvaluations.Where(e => e.Status == status).ToListAsync();
    }

    public async Task AddAsync(PerformanceEvaluation evaluation)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.PerformanceEvaluations.Add(evaluation);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(PerformanceEvaluation evaluation)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        ctx.PerformanceEvaluations.Update(evaluation);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = _contextFactory.CreateDbContext();
        var e = await ctx.PerformanceEvaluations.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException($"PerformanceEvaluation with ID {id} not found");
        ctx.PerformanceEvaluations.Remove(e);
        await ctx.SaveChangesAsync();
    }
}
