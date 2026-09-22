using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Application.DTOs.Catalogue;
using AppPlusPlus.Application.Common;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class CatalogueRepository : RepositoryBase<Article>, ICatalogueRepository
{
    public CatalogueRepository(IDbContextFactory<AppDbContext> dbFactory) : base(dbFactory) { }

    public async Task<Article?> GetByArticleIdAsync(string articleId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Articles.FirstOrDefaultAsync(a => a.IdArticle == articleId);
    }

    public async Task<bool> ArticleExistsAsync(string articleCode)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Articles.AnyAsync(a => a.IdArticle == articleCode);
    }

    public async Task<bool> ArticleDescriptionExistsAsync(string description)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Articles.AnyAsync(a => a.Description == description);
    }

    public async Task<List<Article>> GetArticlesWithStockAsync(List<int> localisationIds)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Articles.AsNoTracking().ToListAsync();
    }

    public async Task<List<ArticleType>> GetAllTypesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ArticleTypes.AsNoTracking().ToListAsync();
    }

    public async Task<List<ArticleMarque>> GetAllMarquesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ArticleMarques.AsNoTracking().ToListAsync();
    }

    public async Task<List<ArticleCategory>> GetAllCategoriesAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.ArticleCategories.AsNoTracking().ToListAsync();
    }

    public async Task<List<Mesure>> GetAllMesuresAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Mesures.AsNoTracking().ToListAsync();
    }

    // CRUD for reference data
    public async Task AddCategoryAsync(ArticleCategory category)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.ArticleCategories.Add(category);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateCategoryAsync(ArticleCategory category)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.ArticleCategories.Update(category);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(ArticleCategory category)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.ArticleCategories.Remove(category);
        await ctx.SaveChangesAsync();
    }

    public async Task AddTypeAsync(ArticleType type)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.ArticleTypes.Add(type);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateTypeAsync(ArticleType type)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.ArticleTypes.Update(type);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteTypeAsync(ArticleType type)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.ArticleTypes.Remove(type);
        await ctx.SaveChangesAsync();
    }

    public async Task AddMarqueAsync(ArticleMarque marque)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.ArticleMarques.Add(marque);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateMarqueAsync(ArticleMarque marque)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.ArticleMarques.Update(marque);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteMarqueAsync(ArticleMarque marque)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.ArticleMarques.Remove(marque);
        await ctx.SaveChangesAsync();
    }

    public async Task AddMesureAsync(Mesure mesure)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Mesures.Add(mesure);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateMesureAsync(Mesure mesure)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Mesures.Update(mesure);
        await ctx.SaveChangesAsync();
    }

    public override async Task UpdateAsync(Article entity)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var existing = await ctx.Articles.FirstOrDefaultAsync(a => a.IdArticle == entity.IdArticle);
        if (existing == null)
            throw new InvalidOperationException($"Article « {entity.IdArticle} » introuvable.");

        existing.Description = entity.Description;
        existing.DescriptionI = entity.DescriptionI;
        existing.DescriptionII = entity.DescriptionII;
        existing.DescriptionIII = entity.DescriptionIII;
        existing.IdMarque = entity.IdMarque;
        existing.IdType = entity.IdType;
        existing.IdCategory = entity.IdCategory;
        existing.IdMesure = entity.IdMesure;
        existing.IdMonais = entity.IdMonais;
        existing.Price = entity.Price;
        // Qte : calculée depuis T_Stock (StockQuantitySync) — ne pas écraser manuellement.
        existing.Soeuil = entity.Soeuil;
        existing.Internal = entity.Internal;
        existing.CanInsertAfter0 = entity.CanInsertAfter0;
        existing.IsTransferable = entity.IsTransferable;
        existing.ImageArticle = string.IsNullOrWhiteSpace(entity.ImageArticle) ? null : entity.ImageArticle.Trim();
        existing.User = entity.User;
        existing.Cumputer = entity.Cumputer;
        existing.DateEditing = entity.DateEditing;

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteMesureAsync(Mesure mesure)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Mesures.Remove(mesure);
        await ctx.SaveChangesAsync();
    }

    public async Task<HashSet<int>> GetCategoryIdsWithOrderableArticlesAsync(int orderableTypeId)
    {
        if (orderableTypeId <= 0)
            return new HashSet<int>();

        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var ids = await (
            from a in ctx.Articles.AsNoTracking()
            join c in ctx.ArticleCategories.AsNoTracking() on a.IdCategory equals c.IdCategory
            where a.IdType == orderableTypeId && c.VisibleCatalog
            select a.IdCategory)
            .Distinct()
            .ToListAsync();

        return ids.ToHashSet();
    }

    public async Task<CatalogBootstrapDto> GetCatalogBootstrapAsync(int orderableTypeId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var categories = await ctx.ArticleCategories
            .AsNoTracking()
            .Where(c => c.VisibleCatalog)
            .OrderBy(c => c.DescriptionCategory)
            .Select(c => new ArticleCategory
            {
                IdCategory = c.IdCategory,
                DescriptionCategory = c.DescriptionCategory,
                VisibleCatalog = c.VisibleCatalog,
                ImageCategory = c.ImageCategory != null && c.ImageCategory != ""
                    ? CatalogImageUrls.Category(c.IdCategory, c.ImageCategory)
                    : null
            })
            .ToListAsync();

        HashSet<int> orderableIds;
        if (orderableTypeId <= 0)
        {
            orderableIds = new HashSet<int>();
        }
        else
        {
            var ids = await (
                from a in ctx.Articles.AsNoTracking()
                join c in ctx.ArticleCategories.AsNoTracking() on a.IdCategory equals c.IdCategory
                where a.IdType == orderableTypeId && c.VisibleCatalog
                select a.IdCategory)
                .Distinct()
                .ToListAsync();
            orderableIds = ids.ToHashSet();
        }

        return new CatalogBootstrapDto
        {
            Categories = categories,
            OrderableCategoryIds = orderableIds
        };
    }

    public async Task<Dictionary<int, List<string>>> GetPreviewImagesByCategoryAsync(int orderableTypeId, int maxPerCategory = 3)
    {
        if (orderableTypeId <= 0 || maxPerCategory <= 0)
            return new Dictionary<int, List<string>>();

        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var visibleCategoryIds = await ctx.ArticleCategories
            .AsNoTracking()
            .Where(c => c.VisibleCatalog)
            .Select(c => c.IdCategory)
            .ToListAsync();

        if (visibleCategoryIds.Count == 0)
            return new Dictionary<int, List<string>>();

        var rows = await ctx.Articles
            .AsNoTracking()
            .Where(a => a.IdType == orderableTypeId
                && visibleCategoryIds.Contains(a.IdCategory)
                && a.ImageArticle != null
                && a.ImageArticle != ""
                && !a.ImageArticle.StartsWith("data:"))
            .OrderByDescending(a => a.DateEditing ?? a.DateSys)
            .Select(a => new { a.IdCategory, a.ImageArticle })
            .Take(visibleCategoryIds.Count * maxPerCategory * 4)
            .ToListAsync();

        return rows
            .GroupBy(r => r.IdCategory)
            .ToDictionary(
                g => g.Key,
                g => g.Take(maxPerCategory).Select(x => x.ImageArticle!).ToList());
    }

    public async Task<(List<CatalogArticleDto> Items, int Total)> GetOrderableArticlesPageAsync(
        int categoryId,
        int orderableTypeId,
        int skip,
        int take,
        string? search = null)
    {
        if (categoryId <= 0 || orderableTypeId <= 0 || take <= 0)
            return ([], 0);

        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var categoryVisible = await ctx.ArticleCategories
            .AsNoTracking()
            .AnyAsync(c => c.IdCategory == categoryId && c.VisibleCatalog);

        if (!categoryVisible)
            return ([], 0);

        var query = ctx.Articles
            .AsNoTracking()
            .Where(a => a.IdCategory == categoryId && a.IdType == orderableTypeId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.Description.Contains(term)
                || (a.DescriptionI != null && a.DescriptionI.Contains(term))
                || a.IdArticle.Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.Description)
            .Skip(skip)
            .Take(take)
            .Select(a => new CatalogArticleDto
            {
                IdArticle = a.IdArticle,
                Description = a.Description,
                ImageArticle = a.ImageArticle != null && a.ImageArticle != ""
                    ? CatalogImageUrls.ArticlePrefix + a.IdArticle
                    : null,
                IdMonais = a.IdMonais,
                Price = a.Price,
                IdCategory = a.IdCategory
            })
            .ToListAsync();

        return (items, total);
    }

    public async Task<(List<CatalogArticleDto> Items, int Total)> SearchOrderableArticlesPageAsync(
        int orderableTypeId,
        int skip,
        int take,
        string search)
    {
        if (orderableTypeId <= 0 || take <= 0 || string.IsNullOrWhiteSpace(search))
            return ([], 0);

        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var term = search.Trim();

        var visibleCategories = await ctx.ArticleCategories
            .AsNoTracking()
            .Where(c => c.VisibleCatalog)
            .Select(c => new { c.IdCategory, c.DescriptionCategory })
            .ToListAsync();

        if (visibleCategories.Count == 0)
            return ([], 0);

        var visibleCategoryIds = visibleCategories.Select(c => c.IdCategory).ToList();
        var categoryNames = visibleCategories.ToDictionary(
            c => c.IdCategory,
            c => c.DescriptionCategory ?? $"Catégorie {c.IdCategory}");

        var query = ctx.Articles
            .AsNoTracking()
            .Where(a => a.IdType == orderableTypeId
                && visibleCategoryIds.Contains(a.IdCategory)
                && (a.Description.Contains(term)
                    || (a.DescriptionI != null && a.DescriptionI.Contains(term))
                    || a.IdArticle.Contains(term)));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.Description)
            .Skip(skip)
            .Take(take)
            .Select(a => new CatalogArticleDto
            {
                IdArticle = a.IdArticle,
                Description = a.Description,
                ImageArticle = a.ImageArticle != null && a.ImageArticle != ""
                    ? CatalogImageUrls.ArticlePrefix + a.IdArticle
                    : null,
                IdMonais = a.IdMonais,
                Price = a.Price,
                IdCategory = a.IdCategory
            })
            .ToListAsync();

        foreach (var item in items)
            item.CategoryName = categoryNames.GetValueOrDefault(item.IdCategory);

        return (items, total);
    }
}
