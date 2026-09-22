using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.DTOs.Inventaire;
using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Application.Services.Inventaire;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Inventaire;
using AppPlusPlus.Infrastructure.Persistence;
using AppPlusPlus.Infrastructure.Services;

namespace AppPlusPlus.Infrastructure.Services;

public class InventairePostingService : IInventairePostingService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly StockMovementService _movements;

    public InventairePostingService(
        IDbContextFactory<AppDbContext> dbFactory,
        StockMovementService movements)
    {
        _dbFactory = dbFactory;
        _movements = movements;
    }

    public async Task CloseReceptionAsync(int receptionId, string userLogin, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await ctx.Database.BeginTransactionAsync(cancellationToken);

        var reception = await ctx.InventaireReceptions
            .Include(r => r.Details)
            .Include(r => r.Inventaire)
            .FirstOrDefaultAsync(r => r.IdReception == receptionId, cancellationToken)
            ?? throw new InvalidOperationException("Réception introuvable.");

        if (reception.Statut == InventaireReceptionStatut.Cloture)
            throw new InvalidOperationException("Cette réception est déjà clôturée.");

        if (reception.Inventaire == null)
            throw new InvalidOperationException("Session inventaire introuvable.");

        if (reception.Inventaire.Statut == InventaireStatut.Cloture)
            throw new InvalidOperationException("La session inventaire est clôturée.");

        if (!reception.Details.Any())
            throw new InvalidOperationException("Ajoutez au moins un article avant de clôturer.");

        var now = DateTime.Now;
        var locId = reception.Inventaire.IdLocalisation;
        var refLabel = reception.NumeroReception ?? reception.Reference ?? $"REC-{receptionId}";

        var alreadyPosted = await ctx.MouvementsStock.AnyAsync(m =>
            m.TypeDocument == TypesDocument.INVENTAIRE &&
            m.IdDocument == receptionId &&
            !m.Annule, cancellationToken);

        if (alreadyPosted)
            await _movements.ReverseDocumentAsync(ctx, TypesDocument.INVENTAIRE, receptionId, userLogin,
                "Réouverture réception inventaire", now, cancellationToken);

        var inboundLines = reception.Details.Select(d => new StockInboundLineRequest(
            d.IdArticle,
            d.Quantite,
            locId,
            d.Id,
            d.PrixUnitaire,
            d.DateExpiration)).ToList();

        await _movements.ApplyInboundAsync(ctx, receptionId, TypesDocument.INVENTAIRE, inboundLines,
            userLogin, refLabel, reception.Observation, now, cancellationToken);

        reception.Statut = InventaireReceptionStatut.Cloture;
        reception.DateCloture = now;
        reception.CloturePar = TextFieldLimits.Truncate(userLogin, TextFieldLimits.MouvementUser);

        await ctx.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }
}
