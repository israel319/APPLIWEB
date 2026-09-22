using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Finance;
using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class FinanceRepository : IFinanceRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public FinanceRepository(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // ── Caisse ──

    public async Task<Caisse?> GetCaisseByIdAsync(int caisseId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Caisses.FindAsync(caisseId);
    }

    public async Task<List<Caisse>> GetAllCaissesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Caisses.ToListAsync();
    }

    public async Task AddCaisseAsync(Caisse caisse)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Caisses.Add(caisse);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateCaisseAsync(Caisse caisse)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Caisses.Update(caisse);
        await ctx.SaveChangesAsync();
    }

    // ── Taux ──

    public async Task<Taux?> GetCurrentTauxAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.TauxChanges.OrderByDescending(t => t.Id).FirstOrDefaultAsync();
    }

    public async Task<List<Taux>> GetAllTauxAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.TauxChanges.OrderByDescending(t => t.Date).ToListAsync();
    }

    public async Task<List<Taux>> GetTauxByDateRangeAsync(DateOnly from, DateOnly to)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.TauxChanges
            .Where(t => t.Date >= from && t.Date <= to)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task AddTauxAsync(Taux taux)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.TauxChanges.Add(taux);
        await ctx.SaveChangesAsync();
    }

    public async Task<Taux?> GetTauxByIdAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.TauxChanges.FindAsync(id);
    }

    public async Task UpdateTauxAsync(Taux taux)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.TauxChanges.Update(taux);
        await ctx.SaveChangesAsync();
    }

    // ── Periode ──

    public async Task<Periode?> GetActivePeriodeAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var today = DateTime.Today;
        return await ctx.Periodes.FirstOrDefaultAsync(p =>
            p.Activated == true &&
            p.FromDate <= today &&
            p.ToDate >= today);
    }

    public async Task<Periode?> GetPeriodeByIdAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Periodes.FindAsync(id);
    }

    public async Task<List<Periode>> GetAllPeriodesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Periodes.OrderByDescending(p => p.FromDate).ToListAsync();
    }

    public async Task AddPeriodeAsync(Periode periode)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Periodes.Add(periode);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdatePeriodeAsync(Periode periode)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Periodes.Update(periode);
        await ctx.SaveChangesAsync();
    }

    // ── Versement ──

    public async Task<bool> VersementExistsAsync(DateOnly date, int localisationId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Versements.AnyAsync(v =>
            v.DateCloture == date && v.LocalisationId == localisationId);
    }

    public async Task<List<Versement>> GetVersementsByDateAsync(DateOnly date)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Versements.Where(v => v.DateCloture == date).ToListAsync();
    }

    public async Task<List<Versement>> GetVersementsByLocalisationAsync(int localisationId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Versements.Where(v => v.LocalisationId == localisationId).ToListAsync();
    }

    public async Task<List<Versement>> GetVersementsByDateRangeAsync(DateOnly from, DateOnly to)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Versements
            .Where(v => v.DateCloture >= from && v.DateCloture <= to)
            .OrderByDescending(v => v.DateCloture)
            .ToListAsync();
    }

    public async Task AddVersementAsync(Versement versement)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var taux = await GetLatestTauxAsync(ctx);
        MonetaryStandard.ApplyVersement(versement, taux);
        ctx.Versements.Add(versement);
        await ctx.SaveChangesAsync();
    }

    // ── ApproExpense ──

    public async Task<List<ApproExpense>> GetApproExpensesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ApproExpenses.ToListAsync();
    }

    public async Task<List<ApproExpense>> GetApproExpensesByCaisseAsync(int caisseId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ApproExpenses.Where(e => e.CaisseId == caisseId).ToListAsync();
    }

    public async Task AddApproExpenseAsync(ApproExpense expense)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var taux = await GetLatestTauxAsync(ctx);
        MonetaryStandard.ApplyApproExpense(expense, taux);
        ctx.ApproExpenses.Add(expense);
        await ctx.SaveChangesAsync();
    }

    public async Task<ApproExpense?> GetApproExpenseByIdAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ApproExpenses.FindAsync(id);
    }

    public async Task UpdateApproExpenseAsync(ApproExpense expense)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var taux = await GetLatestTauxAsync(ctx);
        MonetaryStandard.ApplyApproExpense(expense, taux);
        ctx.ApproExpenses.Update(expense);
        await ctx.SaveChangesAsync();
    }

    static async Task<decimal> GetLatestTauxAsync(AppDbContext ctx) =>
        await ctx.TauxChanges
            .OrderByDescending(t => t.Id)
            .Select(t => t.TauxValue)
            .FirstOrDefaultAsync();

    // ── UserCaisse ──

    public async Task<List<UserCaisse>> GetUserCaissesAsync(string userLogin)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.UserCaisses.Where(uc => uc.User == userLogin).ToListAsync();
    }
}
