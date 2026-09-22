using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.Services;

public class InternalTransferService : IInternalTransferService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly StockMovementService _movements;

    public InternalTransferService(IDbContextFactory<AppDbContext> dbFactory, StockMovementService movements)
    {
        _dbFactory = dbFactory;
        _movements = movements;
    }

    public async Task<int> SaveAsync(InternalTransferSaveRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Lines.Count == 0)
            throw new InvalidOperationException("Sélectionnez au moins un article à transférer.");

        if (string.IsNullOrWhiteSpace(request.UserLogin))
            throw new InvalidOperationException("Utilisateur non identifié.");

        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var reference = TextFieldLimits.Truncate(request.Reference ?? BuildReference(request), TextFieldLimits.ApproReference);
        var commentaire = TextFieldLimits.Truncate(
            string.IsNullOrWhiteSpace(request.Commentaire) ? reference : request.Commentaire,
            TextFieldLimits.ApproCommentaire);
        var user = TextFieldLimits.Truncate(request.UserLogin, TextFieldLimits.ApproUser);
        var machine = TextFieldLimits.Truncate(Environment.MachineName, TextFieldLimits.ApproUser);

        var totalQte = request.Lines.Sum(l => l.Quantity);
        var firstLine = request.Lines[0];

        var appro = new Appro
        {
            IdArticle = firstLine.ArticleId,
            IdCmd = request.SourceCmdId,
            IdCmdDetail = request.Lines.Count == 1 ? (request.Lines[0].CmdDetailId ?? 0) : 0,
            Qte = totalQte,
            PA = 0,
            PV = 0,
            Ben = 0,
            BenTotal = 0,
            Date = today,
            DateSys = now,
            User = user,
            Cumputer = machine,
            Commentaire = commentaire,
            LocalisationId = request.DestLocalisationId,
            StatusId = ApproStatus.Transfer,
            Reference = reference
        };
        ctx.Appros.Add(appro);
        await ctx.SaveChangesAsync(cancellationToken);

        foreach (var line in request.Lines)
        {
            ctx.ApproDetails.Add(new ApproDetail
            {
                IdAppro = appro.Id,
                IdArticle = line.ArticleId,
                IdLocalisation = line.DestLocalisationId,
                Qte = line.Quantity,
                PA = 0,
                PV = 0,
                Ben = 0,
                BenTotal = 0,
                DateSys = now,
                User = user,
                Cumputer = machine
            });
        }

        await _movements.ApplyTransferAsync(
            ctx,
            appro.Id,
            TypesDocument.TRANSFERT,
            request.Lines,
            user,
            reference,
            commentaire,
            now,
            cancellationToken);

        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return appro.Id;
    }

    public async Task CancelAsync(int approId, string userLogin, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        var appro = await ctx.Appros
            .Include(a => a.Details)
            .FirstOrDefaultAsync(a => a.Id == approId, cancellationToken)
            ?? throw new InvalidOperationException("Transfert introuvable.");

        if (!ApproFilters.IsInternalTransfer(appro))
            throw new InvalidOperationException("Cet enregistrement n'est pas un transfert interne.");

        if (ApproFilters.IsCancelled(appro))
            throw new InvalidOperationException("Ce transfert est déjà annulé.");

        var now = DateTime.Now;
        var observation = TextFieldLimits.Truncate(
            $"Annulation transfert N°{approId} par {userLogin}",
            TextFieldLimits.MouvementObservation);

        try
        {
            await _movements.ReverseDocumentAsync(
                ctx, TypesDocument.TRANSFERT, approId, userLogin, observation, now, cancellationToken);
        }
        catch (InvalidOperationException) when (appro.IdCmd.HasValue && appro.IdCmd.Value > 0)
        {
            await _movements.ReverseLegacyCommandTransferAsync(
                ctx,
                appro.IdCmd.Value,
                appro.IdArticle,
                appro.Qte,
                appro.LocalisationId ?? 0,
                appro.DateSys,
                userLogin,
                observation,
                now,
                cancellationToken);
        }

        appro.StatusId = ApproStatus.Cancelled;
        appro.Commentaire = TextFieldLimits.AppendWithLimit(
            appro.Commentaire,
            $"[ANNULÉ {now:dd/MM/yy HH:mm}]",
            TextFieldLimits.ApproCommentaire);

        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(int approId, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        var appro = await ctx.Appros
            .Include(a => a.Details)
            .FirstOrDefaultAsync(a => a.Id == approId, cancellationToken)
            ?? throw new InvalidOperationException("Transfert introuvable.");

        if (!ApproFilters.IsInternalTransfer(appro))
            throw new InvalidOperationException("Cet enregistrement n'est pas un transfert interne.");

        if (!ApproFilters.IsCancelled(appro))
            throw new InvalidOperationException("Annulez d'abord le transfert pour conserver la traçabilité.");

        var details = await ctx.ApproDetails.Where(d => d.IdAppro == approId).ToListAsync(cancellationToken);
        ctx.ApproDetails.RemoveRange(details);
        ctx.Appros.Remove(appro);

        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    static string BuildReference(InternalTransferSaveRequest request)
    {
        if (request.SourceCmdId.HasValue)
            return $"Transfert Cmd N°{request.SourceCmdId}";
        return $"Transfert → loc #{request.DestLocalisationId}";
    }
}
