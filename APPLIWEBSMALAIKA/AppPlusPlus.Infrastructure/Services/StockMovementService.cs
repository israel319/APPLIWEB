using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Stock;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.Services;

/// <summary>
/// Moteur central des mouvements de stock (T_Stock + T_Mouvement_Stock).
/// Toutes les opérations de transfert et contre-passation passent par ici.
/// </summary>
public class StockMovementService
{
    public async Task ApplyTransferAsync(
        AppDbContext ctx,
        int documentId,
        string typeDocument,
        IReadOnlyList<StockTransferLineRequest> lines,
        string userLogin,
        string reference,
        string? observation,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        if (lines.Count == 0)
            throw new InvalidOperationException("Aucune ligne à transférer.");

        reference = TextFieldLimits.Truncate(reference, TextFieldLimits.MouvementReference);
        observation = TextFieldLimits.Truncate(observation, TextFieldLimits.MouvementObservation);
        userLogin = TextFieldLimits.Truncate(userLogin, TextFieldLimits.MouvementUser);

        var today = DateOnly.FromDateTime(now);

        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                throw new InvalidOperationException($"Quantité invalide pour l'article {line.ArticleId}.");

            if (line.SourceLocalisationId == line.DestLocalisationId)
                throw new InvalidOperationException(
                    $"Source et destination identiques pour l'article {line.ArticleId}.");

            var srcStock = await ctx.Stocks.FirstOrDefaultAsync(s =>
                s.IdArticle == line.ArticleId && s.IdLocalisation == line.SourceLocalisationId, cancellationToken);

            var qty = QuantityFormat.Normalize(line.Quantity);

            if (srcStock == null || srcStock.Qte < qty)
                throw new InvalidOperationException(
                    $"Stock insuffisant pour {line.ArticleId} à la localisation #{line.SourceLocalisationId}. " +
                    $"Disponible : {srcStock?.Qte ?? 0}, demandé : {qty}.");
        }

        var touchedArticles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            var qty = QuantityFormat.Normalize(line.Quantity);
            touchedArticles.Add(line.ArticleId);
            var srcStock = await ctx.Stocks.FirstAsync(s =>
                s.IdArticle == line.ArticleId && s.IdLocalisation == line.SourceLocalisationId, cancellationToken);

            var srcQteAvant = srcStock.Qte;
            srcStock.Qte = QuantityFormat.Normalize(srcStock.Qte - qty);
            srcStock.DateSys = today;
            srcStock.UserLogin = userLogin;

            ctx.MouvementsStock.Add(new MouvementStock
            {
                IdArticle = line.ArticleId,
                IdLocalisation = line.SourceLocalisationId,
                TypeMouvement = TypesMouvement.SORTIE,
                Quantite = qty,
                QteAvant = srcQteAvant,
                QteApres = srcStock.Qte,
                DateMouvement = now,
                TypeDocument = typeDocument,
                IdDocument = documentId,
                IdLocalisationSource = line.SourceLocalisationId,
                IdLocalisationDest = line.DestLocalisationId,
                Reference = reference,
                Observation = observation,
                CreePar = userLogin,
                DateCreation = now
            });

            var destStock = await GetOrCreateStockAsync(ctx, line.ArticleId, line.DestLocalisationId, today, userLogin, cancellationToken);

            var destQteAvant = destStock.Qte;
            var destQteApres = QuantityFormat.Normalize(destQteAvant + qty);
            destStock.Qte = destQteApres;
            destStock.DateSys = today;
            destStock.UserLogin = userLogin;

            ctx.MouvementsStock.Add(new MouvementStock
            {
                IdArticle = line.ArticleId,
                IdLocalisation = line.DestLocalisationId,
                TypeMouvement = TypesMouvement.ENTREE,
                Quantite = qty,
                QteAvant = destQteAvant,
                QteApres = destQteApres,
                DateMouvement = now,
                TypeDocument = typeDocument,
                IdDocument = documentId,
                IdLocalisationSource = line.SourceLocalisationId,
                IdLocalisationDest = line.DestLocalisationId,
                Reference = reference,
                Observation = observation,
                CreePar = userLogin,
                DateCreation = now
            });
        }

        await StockQuantitySync.SyncArticlesGlobalAsync(ctx, touchedArticles, cancellationToken);
    }

    public async Task ApplyOutboundAsync(
        AppDbContext ctx,
        int documentId,
        string typeDocument,
        IReadOnlyList<StockOutboundLineRequest> lines,
        string userLogin,
        string reference,
        string? observation,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        if (lines.Count == 0)
            throw new InvalidOperationException("Aucune ligne à sortir du stock.");

        reference = TextFieldLimits.Truncate(reference, TextFieldLimits.MouvementReference);
        observation = TextFieldLimits.Truncate(observation, TextFieldLimits.MouvementObservation);
        userLogin = TextFieldLimits.Truncate(userLogin, TextFieldLimits.MouvementUser);

        var today = DateOnly.FromDateTime(now);
        var touchedArticles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            var qty = QuantityFormat.Normalize(line.Quantity);
            if (qty <= 0)
                throw new InvalidOperationException($"Quantité invalide pour l'article {line.ArticleId}.");

            if (line.LocalisationId <= 0)
                throw new InvalidOperationException($"Localisation requise pour l'article {line.ArticleId}.");

            var stock = await ctx.Stocks.FirstOrDefaultAsync(s =>
                s.IdArticle == line.ArticleId && s.IdLocalisation == line.LocalisationId, cancellationToken);

            if (stock == null || stock.Qte < qty)
                throw new InvalidOperationException(
                    $"Stock insuffisant pour {line.ArticleId} à la localisation #{line.LocalisationId}. " +
                    $"Disponible : {stock?.Qte ?? 0}, demandé : {qty}.");

            touchedArticles.Add(line.ArticleId);
            var qteAvant = stock.Qte;
            stock.Qte = QuantityFormat.Normalize(stock.Qte - qty);
            stock.DateSys = today;
            stock.UserLogin = userLogin;

            ctx.MouvementsStock.Add(new MouvementStock
            {
                IdArticle = line.ArticleId,
                IdLocalisation = line.LocalisationId,
                TypeMouvement = TypesMouvement.SORTIE,
                Quantite = qty,
                QteAvant = qteAvant,
                QteApres = stock.Qte,
                DateMouvement = now,
                TypeDocument = typeDocument,
                IdDocument = documentId,
                IdDocumentDetail = line.IdDocumentDetail,
                Reference = reference,
                PrixUnitaire = line.PrixUnitaire,
                Observation = observation,
                CreePar = userLogin,
                DateCreation = now
            });
        }

        await StockQuantitySync.SyncArticlesGlobalAsync(ctx, touchedArticles, cancellationToken);
    }

    public async Task ApplyInboundAsync(
        AppDbContext ctx,
        int documentId,
        string typeDocument,
        IReadOnlyList<StockInboundLineRequest> lines,
        string userLogin,
        string reference,
        string? observation,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        if (lines.Count == 0)
            throw new InvalidOperationException("Aucune ligne à recevoir en stock.");

        reference = TextFieldLimits.Truncate(reference, TextFieldLimits.MouvementReference);
        observation = TextFieldLimits.Truncate(observation, TextFieldLimits.MouvementObservation);
        userLogin = TextFieldLimits.Truncate(userLogin, TextFieldLimits.MouvementUser);

        var today = DateOnly.FromDateTime(now);
        var touchedArticles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var mergedLines = lines
            .GroupBy(l => (ArticleId: l.ArticleId.Trim(), l.LocalisationId))
            .Select(g => new StockInboundLineRequest(
                g.Key.ArticleId,
                g.Sum(x => x.Quantity),
                g.Key.LocalisationId,
                g.First().IdDocumentDetail,
                g.First().PrixUnitaire,
                g.Select(x => x.DateExpiration).FirstOrDefault(d => d.HasValue)))
            .ToList();

        foreach (var line in mergedLines)
        {
            var qty = QuantityFormat.Normalize(line.Quantity);
            if (qty <= 0)
                throw new InvalidOperationException($"Quantité invalide pour l'article {line.ArticleId}.");

            if (line.LocalisationId <= 0)
                throw new InvalidOperationException($"Localisation requise pour l'article {line.ArticleId}.");

            touchedArticles.Add(line.ArticleId);

            var stock = await GetOrCreateStockAsync(ctx, line.ArticleId, line.LocalisationId, today, userLogin, cancellationToken);

            var qteAvant = stock.Qte;
            var qteApres = QuantityFormat.Normalize(qteAvant + qty);

            stock.Qte = qteApres;
            stock.DateSys = today;
            stock.UserLogin = userLogin;

            ctx.MouvementsStock.Add(new MouvementStock
            {
                IdArticle = line.ArticleId,
                IdLocalisation = line.LocalisationId,
                TypeMouvement = TypesMouvement.ENTREE,
                Quantite = qty,
                QteAvant = qteAvant,
                QteApres = qteApres,
                DateMouvement = now,
                TypeDocument = typeDocument,
                IdDocument = documentId,
                IdDocumentDetail = line.IdDocumentDetail,
                Reference = reference,
                PrixUnitaire = line.PrixUnitaire,
                DateExpiration = line.DateExpiration,
                Observation = observation,
                CreePar = userLogin,
                DateCreation = now
            });
        }

        await StockQuantitySync.SyncArticlesGlobalAsync(ctx, touchedArticles, cancellationToken);
    }

    public async Task ApplyTransformationAsync(
        AppDbContext ctx,
        int documentId,
        StockTransformationRequest request,
        string userLogin,
        string reference,
        string? observation,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        reference = TextFieldLimits.Truncate(reference, TextFieldLimits.MouvementReference);
        observation = TextFieldLimits.Truncate(observation, TextFieldLimits.MouvementObservation);
        userLogin = TextFieldLimits.Truncate(userLogin, TextFieldLimits.MouvementUser);

        var fromQte = QuantityFormat.Normalize(request.FromQuantity);
        var toQte = QuantityFormat.Normalize(request.ToQuantity ?? request.FromQuantity);
        var today = DateOnly.FromDateTime(now);

        if (fromQte <= 0)
            throw new InvalidOperationException("Quantité source invalide.");

        if (request.FromLocalisationId <= 0)
            throw new InvalidOperationException("Localisation source obligatoire.");

        var srcStock = await ctx.Stocks.FirstOrDefaultAsync(s =>
            s.IdArticle == request.FromArticleId && s.IdLocalisation == request.FromLocalisationId, cancellationToken);

        if (srcStock == null || srcStock.Qte < fromQte)
            throw new InvalidOperationException(
                $"Stock insuffisant pour {request.FromArticleId}. Disponible : {srcStock?.Qte ?? 0}, demandé : {fromQte}.");

        var srcQteAvant = srcStock.Qte;
        srcStock.Qte = QuantityFormat.Normalize(srcStock.Qte - fromQte);
        srcStock.DateSys = today;
        srcStock.UserLogin = userLogin;

        ctx.MouvementsStock.Add(new MouvementStock
        {
            IdArticle = request.FromArticleId,
            IdLocalisation = request.FromLocalisationId,
            TypeMouvement = TypesMouvement.SORTIE,
            Quantite = fromQte,
            QteAvant = srcQteAvant,
            QteApres = srcStock.Qte,
            DateMouvement = now,
            TypeDocument = TypesDocument.TRANSFORMATION,
            IdDocument = documentId,
            IdLocalisationSource = request.FromLocalisationId,
            IdLocalisationDest = request.ToLocalisationId,
            Reference = reference,
            Observation = observation,
            CreePar = userLogin,
            DateCreation = now
        });

        var touchedArticles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            request.FromArticleId,
            request.ToArticleId
        };

        if (request.ToLocalisationId.HasValue && request.ToLocalisationId.Value > 0)
        {
            var destLoc = request.ToLocalisationId.Value;
            var destStock = await GetOrCreateStockAsync(ctx, request.ToArticleId, destLoc, today, userLogin, cancellationToken);

            var destQteAvant = destStock.Qte;
            var destQteApres = QuantityFormat.Normalize(destQteAvant + toQte);
            destStock.Qte = destQteApres;
            destStock.DateSys = today;
            destStock.UserLogin = userLogin;

            ctx.MouvementsStock.Add(new MouvementStock
            {
                IdArticle = request.ToArticleId,
                IdLocalisation = destLoc,
                TypeMouvement = TypesMouvement.ENTREE,
                Quantite = toQte,
                QteAvant = destQteAvant,
                QteApres = destQteApres,
                DateMouvement = now,
                TypeDocument = TypesDocument.TRANSFORMATION,
                IdDocument = documentId,
                IdLocalisationSource = request.FromLocalisationId,
                IdLocalisationDest = destLoc,
                Reference = reference,
                Observation = observation,
                CreePar = userLogin,
                DateCreation = now
            });
        }

        await StockQuantitySync.SyncArticlesGlobalAsync(ctx, touchedArticles, cancellationToken);
    }

    public async Task<bool> TryReverseDocumentAsync(
        AppDbContext ctx,
        string typeDocument,
        int documentId,
        string userLogin,
        string observation,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var movements = await ctx.MouvementsStock
            .Where(m => m.TypeDocument == typeDocument &&
                        m.IdDocument == documentId &&
                        !m.Annule)
            .OrderBy(m => m.Id)
            .ToListAsync(cancellationToken);

        if (movements.Count == 0)
            return false;

        await ReverseMovementsAsync(ctx, movements, typeDocument, documentId,
            TextFieldLimits.Truncate(userLogin, TextFieldLimits.MouvementUser),
            TextFieldLimits.Truncate(observation, TextFieldLimits.MouvementObservation),
            now, cancellationToken);

        return true;
    }

    public async Task ReverseDocumentAsync(
        AppDbContext ctx,
        string typeDocument,
        int documentId,
        string userLogin,
        string observation,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var movements = await ctx.MouvementsStock
            .Where(m => m.TypeDocument == typeDocument &&
                        m.IdDocument == documentId &&
                        !m.Annule)
            .OrderBy(m => m.Id)
            .ToListAsync(cancellationToken);

        if (movements.Count == 0)
            throw new InvalidOperationException(
                $"Aucun mouvement de stock actif trouvé pour {typeDocument} #{documentId}.");

        await ReverseMovementsAsync(ctx, movements, typeDocument, documentId,
            TextFieldLimits.Truncate(userLogin, TextFieldLimits.MouvementUser),
            TextFieldLimits.Truncate(observation, TextFieldLimits.MouvementObservation),
            now, cancellationToken);
    }

    public async Task ReverseLegacyCommandTransferAsync(
        AppDbContext ctx,
        int cmdId,
        string articleId,
        decimal quantity,
        int destLocalisationId,
        DateTime movementDate,
        string userLogin,
        string observation,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var day = movementDate.Date;
        var movements = await ctx.MouvementsStock
            .Where(m => m.TypeDocument == TypesDocument.TRANSFERT &&
                        m.IdDocument == cmdId &&
                        m.IdArticle == articleId &&
                        m.Quantite == quantity &&
                        m.IdLocalisationDest == destLocalisationId &&
                        !m.Annule &&
                        m.DateMouvement.Date == day)
            .OrderBy(m => m.Id)
            .ToListAsync(cancellationToken);

        if (movements.Count == 0)
            throw new InvalidOperationException(
                $"Impossible de retrouver les mouvements legacy du transfert (Cmd #{cmdId}, article {articleId}).");

        await ReverseMovementsAsync(ctx, movements, TypesDocument.TRANSFERT, cmdId, userLogin, observation, now, cancellationToken);
    }

    async Task ReverseMovementsAsync(
        AppDbContext ctx,
        List<MouvementStock> movements,
        string typeDocument,
        int documentId,
        string userLogin,
        string observation,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(now);

        foreach (var m in movements)
        {
            if (m.TypeMouvement == TypesMouvement.SORTIE)
            {
                await ApplyStockDeltaAsync(ctx, m.IdArticle, m.IdLocalisation, m.Quantite, today, userLogin, cancellationToken);

                ctx.MouvementsStock.Add(new MouvementStock
                {
                    IdArticle = m.IdArticle,
                    IdLocalisation = m.IdLocalisation,
                    TypeMouvement = TypesMouvement.ENTREE,
                    Quantite = m.Quantite,
                    QteAvant = m.QteApres,
                    QteApres = (m.QteApres ?? 0) + m.Quantite,
                    DateMouvement = now,
                    TypeDocument = typeDocument,
                    IdDocument = documentId,
                    IdLocalisationSource = m.IdLocalisationDest,
                    IdLocalisationDest = m.IdLocalisation,
                    Reference = m.Reference,
                    Observation = observation,
                    CreePar = userLogin,
                    DateCreation = now
                });
            }
            else if (m.TypeMouvement == TypesMouvement.ENTREE)
            {
                var stock = await ctx.Stocks.FirstOrDefaultAsync(s =>
                    s.IdArticle == m.IdArticle && s.IdLocalisation == m.IdLocalisation, cancellationToken);

                if (stock == null || stock.Qte < m.Quantite)
                    throw new InvalidOperationException(
                        $"Stock destination insuffisant pour annuler le transfert de {m.IdArticle} " +
                        $"(disponible : {stock?.Qte ?? 0}, à retirer : {m.Quantite}).");

                var qteAvant = stock.Qte;
                stock.Qte = QuantityFormat.Normalize(stock.Qte - m.Quantite);
                stock.DateSys = today;
                stock.UserLogin = userLogin;

                ctx.MouvementsStock.Add(new MouvementStock
                {
                    IdArticle = m.IdArticle,
                    IdLocalisation = m.IdLocalisation,
                    TypeMouvement = TypesMouvement.SORTIE,
                    Quantite = m.Quantite,
                    QteAvant = qteAvant,
                    QteApres = stock.Qte,
                    DateMouvement = now,
                    TypeDocument = typeDocument,
                    IdDocument = documentId,
                    IdLocalisationSource = m.IdLocalisationSource,
                    IdLocalisationDest = m.IdLocalisationDest,
                    Reference = m.Reference,
                    Observation = observation,
                    CreePar = userLogin,
                    DateCreation = now
                });
            }

            m.Annule = true;
        }

        await StockQuantitySync.SyncArticlesGlobalAsync(
            ctx,
            movements.Select(m => m.IdArticle),
            cancellationToken);
    }

    static async Task ApplyStockDeltaAsync(
        AppDbContext ctx,
        string articleId,
        int localisationId,
        decimal delta,
        DateOnly today,
        string userLogin,
        CancellationToken cancellationToken)
    {
        if (delta == 0) return;

        if (delta > 0)
        {
            var stock = await GetOrCreateStockAsync(ctx, articleId, localisationId, today, userLogin, cancellationToken);
            stock.Qte = QuantityFormat.Normalize(stock.Qte + delta);
            stock.DateSys = today;
            stock.UserLogin = userLogin;
            return;
        }

        var existing = await ctx.Stocks.FirstOrDefaultAsync(s =>
            s.IdArticle == articleId.Trim() && s.IdLocalisation == localisationId, cancellationToken);

        if (existing == null) return;

        existing.Qte = QuantityFormat.Normalize(existing.Qte + delta);
        if (existing.Qte < 0) existing.Qte = 0;
        existing.DateSys = today;
        existing.UserLogin = userLogin;
    }

    static async Task<Stock> GetOrCreateStockAsync(
        AppDbContext ctx,
        string articleId,
        int localisationId,
        DateOnly today,
        string userLogin,
        CancellationToken cancellationToken)
    {
        var normalizedId = articleId.Trim();

        var stock = ctx.Stocks.Local.FirstOrDefault(s =>
            s.IdArticle == normalizedId && s.IdLocalisation == localisationId);

        if (stock == null)
        {
            stock = await ctx.Stocks.FirstOrDefaultAsync(s =>
                s.IdArticle == normalizedId && s.IdLocalisation == localisationId, cancellationToken);
        }

        if (stock != null)
            return stock;

        stock = new Stock
        {
            IdArticle = normalizedId,
            IdLocalisation = localisationId,
            Qte = 0,
            Seuil = 0,
            QteMax = StockDefaults.DefaultQteMax,
            DateSys = today,
            UserLogin = userLogin
        };
        ctx.Stocks.Add(stock);
        return stock;
    }
}
