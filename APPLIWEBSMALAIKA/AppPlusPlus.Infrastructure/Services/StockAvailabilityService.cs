using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.Services;

public class StockAvailabilityService : IStockAvailabilityService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public StockAvailabilityService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<decimal> GetAvailableAsync(
        string articleId,
        int localisationId,
        CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var stock = await ctx.Stocks.AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdArticle == articleId && s.IdLocalisation == localisationId, cancellationToken);
        return StockDomainRules.AvailableAtLocation(stock);
    }

    public async Task<Dictionary<(string ArticleId, int LocalisationId), decimal>> GetAvailableBatchAsync(
        IEnumerable<(string ArticleId, int LocalisationId)> keys,
        CancellationToken cancellationToken = default)
    {
        var keyList = keys.Distinct().ToList();
        if (keyList.Count == 0) return new Dictionary<(string, int), decimal>();

        var articleIds = keyList.Select(k => k.ArticleId).Distinct().ToList();
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var stocks = await ctx.Stocks.AsNoTracking()
            .Where(s => articleIds.Contains(s.IdArticle))
            .ToListAsync(cancellationToken);

        return keyList.ToDictionary(
            k => k,
            k => StockDomainRules.AvailableAtLocation(
                stocks.FirstOrDefault(s => s.IdArticle == k.ArticleId && s.IdLocalisation == k.LocalisationId)));
    }

    public async Task<decimal> GetTransferableFromCommandAsync(
        string articleId,
        int sourceLocalisationId,
        decimal commandRemaining,
        CancellationToken cancellationToken = default)
    {
        var atLoc = await GetAvailableAsync(articleId, sourceLocalisationId, cancellationToken);
        return StockDomainRules.TransferableQuantity(atLoc, commandRemaining);
    }
}
