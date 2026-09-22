using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.Services;

/// <summary>
/// Maintient T_Arts.Qte aligné sur la somme réelle de T_Stock (évite 1500 en appro vs 1490 affiché ailleurs).
/// </summary>
public static class StockQuantitySync
{
    public static async Task SyncArticleGlobalAsync(
        AppDbContext ctx,
        string articleId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(articleId))
            return;

        var total = await ctx.Stocks
            .Where(s => s.IdArticle == articleId)
            .SumAsync(s => s.Qte, cancellationToken);

        total = QuantityFormat.Normalize(total);

        var article = await ctx.Articles.FindAsync([articleId], cancellationToken);
        if (article != null)
            article.Qte = total;
    }

    public static async Task SyncArticlesGlobalAsync(
        AppDbContext ctx,
        IEnumerable<string> articleIds,
        CancellationToken cancellationToken = default)
    {
        foreach (var id in articleIds.Distinct(StringComparer.OrdinalIgnoreCase))
            await SyncArticleGlobalAsync(ctx, id, cancellationToken);
    }
}
