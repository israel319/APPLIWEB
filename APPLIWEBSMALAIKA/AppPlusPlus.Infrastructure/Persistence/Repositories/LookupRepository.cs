using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Entities.Shared;
using AppPlusPlus.Domain.Entities.Finance;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class LookupRepository : ILookupRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public LookupRepository(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Status>> GetAllStatusAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Statuses.ToListAsync();
    }

    public async Task<List<StatusCmd>> GetAllStatusCmdAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.StatusCmds.ToListAsync();
    }

    public async Task<List<StatusFactDetail>> GetAllStatusFactDetailAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.StatusFactDetails.ToListAsync();
    }

    public async Task<List<Currency>> GetAllCurrenciesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Currencies.ToListAsync();
    }

    public async Task<List<Money>> GetAllMoneysAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Moneys.ToListAsync();
    }

    public async Task<Localisation?> GetLocalisationByIdAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Localisations.FindAsync(id);
    }

    public async Task<List<Localisation>> GetAllLocalisationsAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Localisations.ToListAsync();
    }

    public async Task AddLocalisationAsync(Localisation localisation)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        if (localisation.IsDepot)
            await ClearOtherDepotsAsync(ctx, localisation.IdLocalisation);
        ctx.Localisations.Add(localisation);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateLocalisationAsync(Localisation localisation)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        if (localisation.IsDepot)
            await ClearOtherDepotsAsync(ctx, localisation.IdLocalisation);
        ctx.Localisations.Update(localisation);
        await ctx.SaveChangesAsync();
    }

    public async Task<Localisation?> GetDepotLocalisationAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Localisations.FirstOrDefaultAsync(l => l.IsDepot);
    }

    private static async Task ClearOtherDepotsAsync(AppDbContext ctx, int keepId)
    {
        var others = await ctx.Localisations
            .Where(l => l.IsDepot && l.IdLocalisation != keepId)
            .ToListAsync();
        foreach (var loc in others)
            loc.IsDepot = false;
    }

    public async Task DeleteLocalisationAsync(Localisation localisation)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Localisations.Remove(localisation);
        await ctx.SaveChangesAsync();
    }

    public async Task<List<Reduction>> GetAllReductionsAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Reductions.ToListAsync();
    }

    public async Task<List<ExpenseSource>> GetAllExpenseSourcesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ExpenseSources.ToListAsync();
    }
}
