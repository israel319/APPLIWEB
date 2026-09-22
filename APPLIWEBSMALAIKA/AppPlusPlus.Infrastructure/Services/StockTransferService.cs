using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Stock;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.Services;

public class StockTransferService : IStockTransferService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly StockMovementService _movements;

    public StockTransferService(IDbContextFactory<AppDbContext> dbFactory, StockMovementService movements)
    {
        _dbFactory = dbFactory;
        _movements = movements;
    }

    public async Task<int> TransferAsync(
        int sourceLocalisationId,
        int destLocalisationId,
        IEnumerable<StockTransferLine> lines,
        string userLogin,
        string reference,
        string? observation,
        string typeDocument,
        int? documentId)
    {
        if (sourceLocalisationId == destLocalisationId)
            throw new InvalidOperationException("La source et la destination doivent être différentes.");

        var transferLines = lines.Where(l => l.Quantity > 0).ToList();
        if (transferLines.Count == 0)
            return 0;

        var requests = transferLines
            .Select(l => new StockTransferLineRequest(
                l.ArticleId,
                l.Quantity,
                sourceLocalisationId,
                destLocalisationId))
            .ToList();

        await using var ctx = await _dbFactory.CreateDbContextAsync();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        var now = DateTime.Now;
        await _movements.ApplyTransferAsync(
            ctx,
            documentId ?? 0,
            typeDocument,
            requests,
            userLogin,
            reference,
            observation,
            now);

        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
        return transferLines.Count;
    }
}
