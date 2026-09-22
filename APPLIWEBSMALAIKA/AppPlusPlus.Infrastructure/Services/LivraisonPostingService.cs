using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Services.Commandes;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Commandes;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.Services;

public class LivraisonPostingService : ILivraisonPostingService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly StockMovementService _movements;

    public LivraisonPostingService(IDbContextFactory<AppDbContext> dbFactory, StockMovementService movements)
    {
        _dbFactory = dbFactory;
        _movements = movements;
    }

    public async Task<int> SaveAsync(LivraisonSaveRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Lines.Count == 0)
            throw new InvalidOperationException("Ajoutez au moins un article avec une quantité > 0.");

        if (request.LocalisationId <= 0)
            throw new InvalidOperationException("Localisation de livraison requise.");

        foreach (var line in request.Lines)
        {
            if (string.IsNullOrWhiteSpace(line.ArticleId))
                throw new InvalidOperationException("Tous les articles doivent être sélectionnés.");
            line.Qte = QuantityFormat.Normalize(line.Qte);
            if (line.Qte <= 0)
                throw new InvalidOperationException("Les quantités doivent être > 0.");
        }

        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.Now;
        var user = TextFieldLimits.Truncate(request.UserLogin, TextFieldLimits.ApproUser);
        var taux = await ctx.TauxChanges
            .OrderByDescending(t => t.Id)
            .Select(t => t.TauxValue)
            .FirstOrDefaultAsync(cancellationToken);

        Livraison livraison;
        if (request.LivraisonId.HasValue)
        {
            livraison = await ctx.Livraisons
                .Include(l => l.Details)
                .FirstOrDefaultAsync(l => l.LivraisonId == request.LivraisonId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Livraison introuvable.");

            await _movements.TryReverseDocumentAsync(
                ctx,
                TypesDocument.LIVRAISON,
                livraison.LivraisonId,
                user,
                $"Correction livraison N°{livraison.LivraisonId}",
                now,
                cancellationToken);

            var oldDetails = await ctx.LivraisonDetails
                .Where(d => d.LivraisonId == livraison.LivraisonId)
                .ToListAsync(cancellationToken);
            ctx.LivraisonDetails.RemoveRange(oldDetails);
        }
        else
        {
            livraison = new Livraison();
            ctx.Livraisons.Add(livraison);
        }

        livraison.Porteur = request.Porteur;
        livraison.ClientId = request.ClientId;
        livraison.CommandeId = request.CommandeId;
        livraison.Date = request.Date;
        livraison.UserLogin = user;
        livraison.DateSys = now;
        livraison.Status ??= 0;

        await ctx.SaveChangesAsync(cancellationToken);

        var outboundLines = new List<StockOutboundLineRequest>();

        foreach (var line in request.Lines)
        {
            var montantPaye = line.Qte * line.Pu;
            var detail = new LivraisonDetail
            {
                LivraisonId = livraison.LivraisonId,
                CommandDetailId = line.CommandDetailId,
                ArticleId = line.ArticleId,
                Qte = line.Qte,
                LocalisationId = request.LocalisationId,
                Date = now,
                UserLogin = user,
                CreationDate = now
            };
            MonetaryStandard.ApplyLivraisonDetail(detail, montantPaye, taux);
            ctx.LivraisonDetails.Add(detail);
            await ctx.SaveChangesAsync(cancellationToken);

            outboundLines.Add(new StockOutboundLineRequest(
                line.ArticleId,
                line.Qte,
                request.LocalisationId,
                detail.Id,
                line.Pu));
        }

        await _movements.ApplyOutboundAsync(
            ctx,
            livraison.LivraisonId,
            TypesDocument.LIVRAISON,
            outboundLines,
            user,
            $"LIV-{livraison.LivraisonId}",
            $"Livraison N°{livraison.LivraisonId}",
            now,
            cancellationToken);

        if (request.CommandeId.HasValue)
            await UpdateCommandeProgressAsync(ctx, request, cancellationToken);

        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return livraison.LivraisonId;
    }

    static async Task UpdateCommandeProgressAsync(
        AppDbContext ctx,
        LivraisonSaveRequest request,
        CancellationToken cancellationToken)
    {
        var cmdDetails = await ctx.CommandeDetails
            .Where(d => d.CommandeId == request.CommandeId!.Value)
            .ToListAsync(cancellationToken);

        foreach (var line in request.Lines.Where(l => l.CommandDetailId.HasValue))
        {
            var cd = cmdDetails.FirstOrDefault(d => d.Id == line.CommandDetailId!.Value);
            if (cd == null) continue;

            cd.QtePrise = (cd.QtePrise ?? 0) + line.Qte;
            cd.QteRest = (cd.Qte ?? 0) - (cd.QtePrise ?? 0);
            if (cd.QteRest < 0) cd.QteRest = 0;
        }

        var commande = await ctx.Commandes.FindAsync([request.CommandeId!.Value], cancellationToken);
        if (commande != null)
        {
            var toutLivre = cmdDetails.All(d => (d.QteRest ?? 0) <= 0);
            commande.Status = toutLivre ? 2 : 1;
        }
    }

    public async Task DeleteAsync(int livraisonId, string userLogin, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        var livraison = await ctx.Livraisons
            .FirstOrDefaultAsync(l => l.LivraisonId == livraisonId, cancellationToken)
            ?? throw new InvalidOperationException("Livraison introuvable.");

        var now = DateTime.Now;
        await _movements.TryReverseDocumentAsync(
            ctx,
            TypesDocument.LIVRAISON,
            livraisonId,
            userLogin,
            $"Suppression livraison N°{livraisonId}",
            now,
            cancellationToken);

        var details = await ctx.LivraisonDetails.Where(d => d.LivraisonId == livraisonId).ToListAsync(cancellationToken);
        ctx.LivraisonDetails.RemoveRange(details);
        ctx.Livraisons.Remove(livraison);

        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }
}
