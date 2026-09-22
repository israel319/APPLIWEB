using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Common;
using AppPlusPlus.Application.Services.Approvisionnement;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.Services;

public class TransformationPostingService : ITransformationPostingService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly StockMovementService _movements;

    public TransformationPostingService(IDbContextFactory<AppDbContext> dbFactory, StockMovementService movements)
    {
        _dbFactory = dbFactory;
        _movements = movements;
    }

    public async Task<ServiceResult> PostAsync(Transformation transformation, CancellationToken cancellationToken = default)
    {
        if (!transformation.FromLocalisationId.HasValue)
            return ServiceResult.Fail("La localisation source est obligatoire.");

        var fromQte = QuantityFormat.Normalize(transformation.Qte);
        if (fromQte <= 0)
            return ServiceResult.Fail("La quantité doit être > 0.");

        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.Now;
        transformation.Qte = fromQte;
        transformation.ToQte = transformation.ToQte.HasValue
            ? QuantityFormat.Normalize(transformation.ToQte.Value)
            : fromQte;
        transformation.DateSys = now;
        transformation.UserLogin = TextFieldLimits.Truncate(transformation.UserLogin, TextFieldLimits.ApproUser);
        transformation.Computer = TextFieldLimits.Truncate(transformation.Computer, TextFieldLimits.ApproUser);
        transformation.Comment = TextFieldLimits.Truncate(transformation.Comment, TextFieldLimits.MouvementObservation);

        ctx.Transformations.Add(transformation);
        await ctx.SaveChangesAsync(cancellationToken);

        var reference = $"TRANSFO-{transformation.TransformationId}";
        await _movements.ApplyTransformationAsync(
            ctx,
            transformation.TransformationId,
            new StockTransformationRequest(
                transformation.FromArticleId,
                transformation.ToArticleId,
                fromQte,
                transformation.ToQte,
                transformation.FromLocalisationId.Value,
                transformation.ToLocalisationId),
            transformation.UserLogin,
            reference,
            transformation.Comment,
            now,
            cancellationToken);

        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return ServiceResult.Ok("Transformation enregistrée avec succès.");
    }
}
