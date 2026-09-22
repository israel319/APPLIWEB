using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.DTOs.Stock;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Infrastructure.Persistence;
using StockEntity = AppPlusPlus.Domain.Entities.Stock.Stock;

namespace AppPlusPlus.Infrastructure.QueryServices;

public class StockCatalogQueryService : IStockCatalogQueryService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public StockCatalogQueryService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<StockCatalogQueryResult> GetCatalogAsync(
        IReadOnlyList<int> localisationIds,
        CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var stocksQuery = ctx.Stocks.AsNoTracking().AsQueryable();
        if (localisationIds.Count > 0)
            stocksQuery = stocksQuery.Where(s => localisationIds.Contains(s.IdLocalisation));

        var stocks = await stocksQuery.ToListAsync(cancellationToken);
        var articleIds = stocks.Select(s => s.IdArticle).Distinct().ToList();

        var articles = await ctx.Articles.AsNoTracking()
            .Where(a => articleIds.Contains(a.IdArticle))
            .OrderBy(a => a.Description)
            .ToListAsync(cancellationToken);

        var moneyById = await ctx.Moneys.AsNoTracking()
            .ToDictionaryAsync(m => m.IdMonais, m => m.DescriptionMonais, cancellationToken);

        var categoryNames = await ctx.ArticleCategories.AsNoTracking()
            .ToDictionaryAsync(c => c.IdCategory, c => c.DescriptionCategory ?? "", cancellationToken);

        var typeNames = await ctx.ArticleTypes.AsNoTracking()
            .ToDictionaryAsync(t => t.IdType, t => t.DescriptionType, cancellationToken);

        var dtos = articles.Select(a =>
        {
            var articleStocks = stocks.Where(s => s.IdArticle == a.IdArticle).ToList();
            var typeDescription = typeNames.GetValueOrDefault(a.IdType);
            var totalStockDec = StockDomainRules.TotalQuantity(articleStocks);

            return new StockCatalogArticleDto
            {
                IdArticle = a.IdArticle,
                Description = a.Description ?? a.IdArticle,
                DescriptionI = a.DescriptionI,
                Price = a.Price,
                IsUsd = CurrencyFormat.IsUsd(a.IdMonais, moneyById),
                IdMarque = a.IdMarque,
                IdCategory = a.IdCategory,
                CategoryName = categoryNames.GetValueOrDefault(a.IdCategory, $"Catégorie {a.IdCategory}"),
                IdType = a.IdType,
                TypeName = string.IsNullOrWhiteSpace(typeDescription) ? $"Type {a.IdType}" : typeDescription,
                IsOrderableOnline = ArticleTypeCatalog.IsOrderableDescription(typeDescription),
                IdMesure = a.IdMesure,
                LocationCount = articleStocks.Count,
                TotalStock = QuantityFormat.Normalize(totalStockDec),
                TotalBudgetQty = StockDomainRules.TotalBudgetQuantity(articleStocks),
                BudgetRestantQty = StockDomainRules.RemainingBudgetQuantity(articleStocks),
                HasRupture = StockDomainRules.IsOutOfStock(articleStocks),
                HasLowStock = StockDomainRules.IsLowStock(articleStocks)
            };
        }).ToList();

        return new StockCatalogQueryResult { Articles = dtos, Stocks = stocks };
    }
}
