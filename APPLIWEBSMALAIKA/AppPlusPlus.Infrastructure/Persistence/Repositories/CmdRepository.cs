using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Entities.CommandesInternes;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class CmdRepository : RepositoryBase<Cmd>, ICmdRepository
{
    public CmdRepository(IDbContextFactory<AppDbContext> dbFactory) : base(dbFactory) { }

    public async Task<Cmd?> GetWithDetailsAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Cmds
            .Include(c => c.Supplier)
            .Include(c => c.Details).ThenInclude(d => d.Article)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Cmd>> GetBySupplierAsync(int supplierId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Cmds
            .Where(c => c.SupplierId == supplierId)
            .Include(c => c.Details)
            .ToListAsync();
    }

    public async Task<List<Cmd>> GetByStatusAsync(int status)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Cmds
            .Where(c => c.Status == status)
            .Include(c => c.Details)
            .ToListAsync();
    }

    public async Task<List<Cmd>> GetByDateRangeAsync(DateOnly from, DateOnly to)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Cmds
            .Where(c => c.DateCommande >= from && c.DateCommande <= to)
            .Include(c => c.Details)
            .OrderByDescending(c => c.DateCommande)
            .ToListAsync();
    }

    public async Task<List<Cmd>> GetByUserAsync(string userLogin)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Cmds
            .Where(c => c.User == userLogin)
            .Include(c => c.Details)
            .ToListAsync();
    }

    public async Task<List<CmdDetail>> GetDetailsByCmdIdAsync(int cmdId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.CmdDetails
            .Where(d => d.IdCmd == cmdId)
            .ToListAsync();
    }

    public async Task<List<Cmd>> GetAllWithDetailsAndSupplierAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Cmds
            .Include(c => c.Supplier)
            .Include(c => c.Localisation)
            .Include(c => c.Details)
            .OrderByDescending(c => c.DateCommande)
            .ToListAsync();
    }

    public async Task UpdateCmdStatusBatchAsync(List<Cmd> cmds)
    {
        if (!cmds.Any()) return;

        await using var ctx = await _dbFactory.CreateDbContextAsync();
        foreach (var cmd in cmds)
        {
            ctx.Attach(cmd);
            ctx.Entry(cmd).Property(c => c.Status).IsModified = true;
        }
        await ctx.SaveChangesAsync();
    }
}
