using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Demandes;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class DemandeRepository : IDemandeRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public DemandeRepository(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Demande>> GetAllWithDetailsAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await QueryWithIncludes(ctx)
            .OrderByDescending(d => d.DateCreation)
            .ToListAsync();
    }

    public async Task<Demande?> GetByIdWithDetailsAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await QueryWithIncludes(ctx)
            .FirstOrDefaultAsync(d => d.IdDemande == id);
    }

    public async Task<List<Demande>> GetForUserAsync(bool hasGlobalScope, List<int> userLocIds)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var query = QueryWithIncludes(ctx);

        if (hasGlobalScope)
        {
            query = query.Where(d =>
                d.Statut == DemandeStatut.EnAttenteAdmin
                || d.Statut == DemandeStatut.EnAttenteAgent
                || d.Statut == DemandeStatut.Completee
                || d.Statut == DemandeStatut.RefuseeAdmin
                || d.Statut == DemandeStatut.RefuseeAgent);
        }
        else if (userLocIds.Any())
        {
            query = query.Where(d =>
                userLocIds.Contains(d.IdLocalisationDemandeur)
                && (d.Statut == DemandeStatut.EnAttenteAgent
                    || d.Statut == DemandeStatut.Completee
                    || d.Statut == DemandeStatut.RefuseeAdmin
                    || d.Statut == DemandeStatut.RefuseeAgent));
        }
        else
        {
            return new List<Demande>();
        }

        return await query
            .OrderByDescending(d => d.DateCreation)
            .ToListAsync();
    }

    public async Task AddAsync(Demande demande)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Demandes.Add(demande);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Demande demande)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Demandes.Update(demande);
        await ctx.SaveChangesAsync();
    }

    private static IQueryable<Demande> QueryWithIncludes(AppDbContext ctx)
        => ctx.Demandes
            .Include(d => d.LocalisationDemandeur)
            .Include(d => d.LocalisationSource)
            .Include(d => d.Details)
                .ThenInclude(dd => dd.Article);
}
