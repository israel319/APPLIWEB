using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class ApproRepository : RepositoryBase<Appro>, IApproRepository
{
    public ApproRepository(IDbContextFactory<AppDbContext> dbFactory) : base(dbFactory) { }

    public async Task<Appro?> GetWithDetailsAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Appros
            .Include(a => a.Details)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Appro>> GetByDateRangeAsync(DateOnly from, DateOnly to)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Appros
            .Where(a => a.Date >= from && a.Date <= to)
            .Include(a => a.Details)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<List<Appro>> GetBySupplierAsync(int supplierId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Appros
            .Where(a => a.SupplierId == supplierId)
            .Include(a => a.Details)
            .ToListAsync();
    }

    public async Task<List<Appro>> GetByLocalisationAsync(int localisationId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Appros
            .Where(a => a.LocalisationId == localisationId)
            .Include(a => a.Details)
            .ToListAsync();
    }

    public async Task<List<Appro>> GetByUserAsync(string userLogin)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Appros
            .Where(a => a.User == userLogin)
            .Include(a => a.Details)
            .ToListAsync();
    }

    public async Task<List<ApproDetail>> GetDetailsByApproIdAsync(int approId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ApproDetails
            .Where(d => d.IdAppro == approId)
            .ToListAsync();
    }

    // ── Transformations ──

    public async Task<Transformation?> GetTransformationByIdAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Transformations.FindAsync(id);
    }

    public async Task<List<Transformation>> GetTransformationsByLocalisationAsync(int localisationId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Transformations
            .Where(t => t.FromLocalisationId == localisationId || t.ToLocalisationId == localisationId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task AddTransformationAsync(Transformation transformation)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Transformations.Add(transformation);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateTransformationAsync(Transformation transformation)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Transformations.Update(transformation);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteTransformationAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var entity = await ctx.Transformations.FindAsync(id);
        if (entity != null)
        {
            ctx.Transformations.Remove(entity);
            await ctx.SaveChangesAsync();
        }
    }
}
