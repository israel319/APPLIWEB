using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Inventaire;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class InventaireRepository : IInventaireRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public InventaireRepository(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<InventaireMagasin>> GetSessionsForLocalisationsAsync(
        IReadOnlyList<int> locIds, bool hasGlobalScope)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var query = SessionQuery(ctx);

        if (!hasGlobalScope)
        {
            if (locIds.Count == 0)
                return new List<InventaireMagasin>();
            query = query.Where(s => locIds.Contains(s.IdLocalisation));
        }

        return await query
            .OrderByDescending(s => s.DateCreation)
            .Take(200)
            .ToListAsync();
    }

    public async Task<InventaireMagasin?> GetSessionByIdAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await SessionQuery(ctx)
            .FirstOrDefaultAsync(s => s.IdInventaire == id);
    }

    public async Task<InventaireMagasin?> GetOpenSessionForLocalisationAsync(int localisationId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await SessionQuery(ctx)
            .Where(s => s.IdLocalisation == localisationId && s.Statut != InventaireStatut.Cloture)
            .OrderByDescending(s => s.DateCreation)
            .FirstOrDefaultAsync();
    }

    public async Task<InventaireMagasin?> GetLastClosedSessionForLocalisationAsync(int localisationId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await SessionQuery(ctx)
            .Where(s => s.IdLocalisation == localisationId && s.Statut == InventaireStatut.Cloture)
            .OrderByDescending(s => s.DateFin ?? s.DateDebut)
            .ThenByDescending(s => s.DateCloture)
            .FirstOrDefaultAsync();
    }

    public async Task<List<InventaireMagasin>> GetClosedSessionsForLocalisationAsync(int localisationId, int take = 200)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await SessionQuery(ctx)
            .Where(s => s.IdLocalisation == localisationId && s.Statut == InventaireStatut.Cloture)
            .OrderByDescending(s => s.DateFin ?? s.DateDebut)
            .ThenByDescending(s => s.DateCloture)
            .Take(take)
            .ToListAsync();
    }

    public async Task<InventaireReception?> GetReceptionByIdWithDetailsAsync(int receptionId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.InventaireReceptions
            .Include(r => r.Details).ThenInclude(d => d.Article)
            .Include(r => r.Inventaire)!.ThenInclude(i => i!.Localisation)
            .FirstOrDefaultAsync(r => r.IdReception == receptionId);
    }

    public async Task<InventaireReception?> GetReceptionForDayAsync(int inventaireId, DateOnly dateJour)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.InventaireReceptions
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.IdInventaire == inventaireId && r.DateJour == dateJour);
    }

    public async Task<List<InventaireReception>> GetReceptionHistoryForLocalisationAsync(int localisationId, int take = 500)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.InventaireReceptions
            .Include(r => r.Details)
            .Include(r => r.Inventaire)
            .Where(r => r.Inventaire != null && r.Inventaire.IdLocalisation == localisationId)
            .OrderByDescending(r => r.DateJour)
            .ThenByDescending(r => r.DateCreation)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<InventaireReception>> GetReceptionsForSessionAsync(int inventaireId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.InventaireReceptions
            .Include(r => r.Details)
            .Where(r => r.IdInventaire == inventaireId)
            .OrderByDescending(r => r.DateJour)
            .ThenByDescending(r => r.DateCreation)
            .ToListAsync();
    }

    public async Task<List<InventaireVente>> GetVentesForSessionAsync(int inventaireId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.InventaireVentes
            .Where(v => v.IdInventaire == inventaireId)
            .OrderByDescending(v => v.DateVente)
            .ToListAsync();
    }

    public async Task<InventaireVente?> GetVenteForDayAsync(int inventaireId, DateOnly dateVente)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.InventaireVentes
            .FirstOrDefaultAsync(v => v.IdInventaire == inventaireId && v.DateVente == dateVente);
    }

    public async Task AddSessionAsync(InventaireMagasin session)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.InventairesMagasin.Add(session);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateSessionAsync(InventaireMagasin session)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.InventairesMagasin.Update(session);
        await ctx.SaveChangesAsync();
    }

    public async Task AddReceptionAsync(InventaireReception reception)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.InventaireReceptions.Add(reception);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateReceptionAsync(InventaireReception reception)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var entity = await ctx.InventaireReceptions.FindAsync(reception.IdReception)
                     ?? throw new InvalidOperationException("Réception introuvable.");

        entity.NumeroReception = reception.NumeroReception;
        entity.Reference = reception.Reference;
        entity.Observation = reception.Observation;
        entity.Statut = reception.Statut;
        entity.DateCloture = reception.DateCloture;
        entity.CloturePar = reception.CloturePar;

        await ctx.SaveChangesAsync();
    }

    public async Task SaveReceptionDetailsAsync(int receptionId, List<InventaireReceptionDetail> details)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var existing = await ctx.InventaireReceptionDetails
            .Where(d => d.IdReception == receptionId)
            .ToListAsync();

        ctx.InventaireReceptionDetails.RemoveRange(existing);

        foreach (var d in details)
        {
            ctx.InventaireReceptionDetails.Add(new InventaireReceptionDetail
            {
                IdReception = receptionId,
                IdArticle = d.IdArticle,
                Quantite = d.Quantite,
                PrixUnitaire = d.PrixUnitaire,
                DateLigne = d.DateLigne,
                Observation = d.Observation
            });
        }

        await ctx.SaveChangesAsync();
    }

    public async Task UpsertVenteAsync(InventaireVente vente)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var existing = await ctx.InventaireVentes
            .FirstOrDefaultAsync(v => v.IdInventaire == vente.IdInventaire && v.DateVente == vente.DateVente);

        if (existing != null)
        {
            existing.MontantVentes = vente.MontantVentes;
            existing.MontantDepenses = vente.MontantDepenses;
            existing.Montant = vente.MontantVentes + vente.MontantDepenses;
            existing.Observation = vente.Observation;
            ctx.InventaireVentes.Update(existing);
        }
        else
        {
            vente.Montant = vente.MontantVentes + vente.MontantDepenses;
            ctx.InventaireVentes.Add(vente);
        }

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteVenteAsync(int venteId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var vente = await ctx.InventaireVentes.FindAsync(venteId);
        if (vente != null)
        {
            ctx.InventaireVentes.Remove(vente);
            await ctx.SaveChangesAsync();
        }
    }

    static IQueryable<InventaireMagasin> SessionQuery(AppDbContext ctx)
        => ctx.InventairesMagasin
            .Include(s => s.Localisation)
            .Include(s => s.Receptions)
            .Include(s => s.Ventes);
}
