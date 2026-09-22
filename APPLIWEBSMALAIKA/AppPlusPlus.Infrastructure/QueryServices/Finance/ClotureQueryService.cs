using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Common;
using AppPlusPlus.Application.DTOs.Finance;
using AppPlusPlus.Application.Services.Finance;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Domain.Entities.Finance;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.QueryServices.Finance;

public class ClotureQueryService : IClotureService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ClotureQueryService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Versement>> GetCloturesByLocalisationsAsync(List<int> localisationIds)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var query = ctx.Versements
            .Include(v => v.Localisation)
            .AsQueryable();

        if (localisationIds.Any())
        {
            query = query.Where(v => localisationIds.Contains(v.LocalisationId));
        }

        return await query
            .OrderByDescending(v => v.DateCloture)
            .ThenByDescending(v => v.HeureCloture)
            .ToListAsync();
    }

    public async Task<bool> ClotureExistsAsync(DateOnly date, int localisationId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Versements.AnyAsync(v =>
            v.DateCloture == date
            && v.LocalisationId == localisationId
            && v.StatutCloture == 0);
    }

    public async Task<ClotureSummaryDto> GetClotureSummaryAsync(DateOnly date, int localisationId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var dto = new ClotureSummaryDto();

        // Check if cloture already exists
        dto.DejaClotureExiste = await ctx.Versements.AnyAsync(v =>
            v.DateCloture == date
            && v.LocalisationId == localisationId
            && v.StatutCloture == 0);

        // Factures validées/payées du jour, filtrées par localisation des lignes
        var factures = await ctx.Facts
            .Include(f => f.Details)
            .Where(f => f.Status >= 1 && f.Status != 3
                && f.Date == date
                && f.Details.Any(d => d.Localisationid == localisationId || !d.Localisationid.HasValue))
            .ToListAsync();

        var versements = await ctx.Versements
            .Where(v => v.DateCloture == date && v.LocalisationId == localisationId)
            .ToListAsync();

        var cutoff = ClotureSegmentHelper.GetLastApprovedCutoff(date, localisationId, versements);
        dto.ApresCloturePrecedente = cutoff.HasValue;
        if (cutoff.HasValue)
            factures = factures.Where(f => f.DateSys > cutoff.Value).ToList();

        dto.NbFactures = factures.Count;
        dto.TotalFactures = factures.Sum(f => f.TotalApresReduction ?? f.Total ?? 0);

        // Payments for the date, filtered through Fact -> FactDetail.Localisationid
        var factIds = factures.Select(f => f.Id).ToHashSet();
        var paiements = await ctx.Payments
            .Include(p => p.Fact).ThenInclude(f => f!.Details)
            .Where(p => p.Date == date
                && p.Fact != null
                && factIds.Contains(p.IdFact)
                && p.Fact.Details.Any(d => d.Localisationid == localisationId || !d.Localisationid.HasValue))
            .ToListAsync();

        dto.NbPaiements = paiements.Count;
        dto.TotalPaiements = paiements.Sum(p => p.Montant);

        // Une vente = 1 facture (le paiement auto associé ne compte pas en plus)
        var venteIds = factures.Select(f => f.Id).ToHashSet();
        foreach (var p in paiements)
            venteIds.Add(p.IdFact);
        dto.NbOperations = venteIds.Count;

        return dto;
    }

    public async Task CreateClotureAsync(Versement versement)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        // Créer la clôture (Versement) — l'entrée caisse est créée uniquement après approbation admin
        ctx.Versements.Add(versement);
        await ctx.SaveChangesAsync();
    }

    public async Task<bool> HasUserClosedTodayAsync(string userLogin, List<int> localisationIds)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await ctx.Versements.AnyAsync(v =>
            v.UserLogin == userLogin
            && v.DateCloture == today
            && localisationIds.Contains(v.LocalisationId)
            && v.StatutCloture == 0);
    }

    public async Task<bool> UpdateClotureStatutAsync(int versementId, int statut, string traitePar, string? motifRejet = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var rows = await ctx.Versements
            .Where(v => v.Id == versementId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(v => v.StatutCloture, statut)
                .SetProperty(v => v.TraitePar, traitePar)
                .SetProperty(v => v.DateTraitement, DateTime.Now)
                .SetProperty(v => v.MotifRejet, statut == 2 ? motifRejet : null));

        return rows > 0;
    }

    public async Task<int?> GetSuggestedLocalisationForClotureAsync(DateOnly date, List<int> localisationIds)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        if (!localisationIds.Any())
        {
            localisationIds = await ctx.Localisations
                .Select(l => l.IdLocalisation)
                .ToListAsync();
        }

        if (!localisationIds.Any()) return null;

        int? bestLoc = null;
        decimal bestTotal = -1;

        foreach (var locId in localisationIds)
        {
            var summary = await GetClotureSummaryAsync(date, locId);
            var total = summary.MontantTotal;
            if (total > bestTotal)
            {
                bestTotal = total;
                bestLoc = locId;
            }
        }

        return bestLoc ?? localisationIds[0];
    }

    public async Task<Dictionary<int, ClotureSummaryDto>> GetClotureSummariesAsync(DateOnly date, IEnumerable<int> localisationIds)
    {
        var result = new Dictionary<int, ClotureSummaryDto>();
        foreach (var locId in localisationIds.Distinct())
            result[locId] = await GetClotureSummaryAsync(date, locId);
        return result;
    }

    public async Task EnsureClotureExpenseAsync(int versementId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var versement = await ctx.Versements.FindAsync(versementId);
        if (versement == null || versement.Montant <= 0) return;

        var exists = await ctx.ApproExpenses.AnyAsync(e => e.VersementId == versementId);
        if (exists) return;

        var source = await ctx.Set<ExpenseSource>()
            .FirstOrDefaultAsync(s => s.Sources == "Clôture journalière");
        if (source == null)
        {
            source = new ExpenseSource { Sources = "Clôture journalière" };
            ctx.Set<ExpenseSource>().Add(source);
            await ctx.SaveChangesAsync();
        }

        var locName = await ctx.Localisations
            .Where(l => l.IdLocalisation == versement.LocalisationId)
            .Select(l => l.DescriptionLocalisation)
            .FirstOrDefaultAsync() ?? "";

        var taux = await ctx.TauxChanges
            .OrderByDescending(t => t.Id)
            .Select(t => t.TauxValue)
            .FirstOrDefaultAsync();

        var expense = new ApproExpense
        {
            SourceId = source.Id,
            VersementId = versement.Id,
            AmountCDF = versement.Montant,
            Description = $"Clôture {locName} — {versement.DateCloture:dd/MM/yyyy}",
            Depositeur = versement.UserLogin,
            Comment = versement.Observation,
            UserLogin = versement.UserLogin,
            CreationDate = DateTime.Now,
        };
        MonetaryStandard.ApplyApproExpense(expense, taux);
        ctx.ApproExpenses.Add(expense);
        await ctx.SaveChangesAsync();
    }
}
