using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.Services;

public class StockDocumentReversalService : IStockDocumentReversal
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly StockMovementService _movements;

    public StockDocumentReversalService(IDbContextFactory<AppDbContext> dbFactory, StockMovementService movements)
    {
        _dbFactory = dbFactory;
        _movements = movements;
    }

    public async Task ReverseTransformationAsync(int transformationId, string userLogin, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        var exists = await ctx.Transformations.AnyAsync(t => t.TransformationId == transformationId, cancellationToken);
        if (!exists)
            throw new InvalidOperationException("Transformation introuvable.");

        var now = DateTime.Now;
        await _movements.ReverseDocumentAsync(
            ctx,
            TypesDocument.TRANSFORMATION,
            transformationId,
            userLogin,
            $"Annulation transformation N°{transformationId}",
            now,
            cancellationToken);

        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }
}
