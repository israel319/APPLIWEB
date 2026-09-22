using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Domain.Enums;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class FactureRepository : RepositoryBase<Fact>, IFactureRepository
{
    public FactureRepository(IDbContextFactory<AppDbContext> dbFactory) : base(dbFactory) { }

    public async Task<Fact?> GetWithDetailsAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Facts
            .Include(f => f.Details)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Fact?> GetWithDetailsAndPaymentsAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Facts
            .Include(f => f.Details)
            .Include(f => f.Payments)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<List<Fact>> GetByDateRangeAsync(DateOnly from, DateOnly to)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Facts
            .Where(f => f.Date >= from && f.Date <= to)
            .Include(f => f.Details)
            .OrderByDescending(f => f.Date)
            .ToListAsync();
    }

    public async Task<List<Fact>> GetByStatusAsync(FactureStatus status)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Facts
            .Where(f => f.Status == (int)status)
            .Include(f => f.Details)
            .ToListAsync();
    }

    public async Task<List<Fact>> GetByUserAsync(string userLogin)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Facts
            .Where(f => f.DescriptionName == userLogin)
            .Include(f => f.Details)
            .OrderByDescending(f => f.Date)
            .ToListAsync();
    }

    public async Task<List<Fact>> GetByCustomerAsync(int customerId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Facts
            .Where(f => f.CustomerId == customerId)
            .Include(f => f.Details)
            .OrderByDescending(f => f.Date)
            .ToListAsync();
    }

    public async Task<List<Fact>> GetByLocalisationIdsAsync(List<int> localisationIds)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Facts
            .Where(f => f.Details.Any(d => d.Localisationid.HasValue && localisationIds.Contains(d.Localisationid.Value)))
            .Include(f => f.Details)
            .OrderByDescending(f => f.Date)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetPaymentsByFactIdAsync(int factId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Payments.Where(p => p.IdFact == factId).ToListAsync();
    }

    public async Task AddPaymentAsync(Payment payment)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Payments.Add(payment);
        await ctx.SaveChangesAsync();
    }
}
