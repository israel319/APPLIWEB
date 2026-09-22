using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Application.DTOs.Catalogue;
using Microsoft.Extensions.Caching.Memory;
using StockEntity = AppPlusPlus.Domain.Entities.Stock.Stock;

namespace AppPlusPlus.Application.Services.Catalogue;

public class CatalogueService : ICatalogueService
{
    private readonly ICatalogueRepository _catalogueRepo;
    private readonly ILookupRepository _lookupRepo;
    private readonly IStockRepository _stockRepo;
    private readonly IMemoryCache _cache;
    int? _orderableTypeId;

    const string CatalogBootstrapCacheKey = "catalog:bootstrap:v2";
    static readonly TimeSpan CatalogBootstrapCacheDuration = TimeSpan.FromMinutes(15);

    public CatalogueService(
        ICatalogueRepository catalogueRepo,
        ILookupRepository lookupRepo,
        IStockRepository stockRepo,
        IMemoryCache cache)
    {
        _catalogueRepo = catalogueRepo;
        _lookupRepo = lookupRepo;
        _stockRepo = stockRepo;
        _cache = cache;
    }

    // Articles
    public async Task<List<Article>> GetArticlesAsync()
        => await _catalogueRepo.GetAllAsync();

    public async Task<int> GetOrderableArticleTypeIdAsync()
    {
        if (_orderableTypeId.HasValue)
            return _orderableTypeId.Value;

        var types = await _catalogueRepo.GetAllTypesAsync();
        _orderableTypeId = types
            .FirstOrDefault(t => ArticleTypeCatalog.IsOrderableDescription(t.DescriptionType))
            ?.IdType ?? 0;

        return _orderableTypeId.Value;
    }

    public async Task<CatalogBootstrapDto> GetCatalogBootstrapAsync()
    {
        if (_cache.TryGetValue(CatalogBootstrapCacheKey, out CatalogBootstrapDto? cached) && cached is not null)
            return cached;

        var typeId = await GetOrderableArticleTypeIdAsync();
        var bootstrap = await _catalogueRepo.GetCatalogBootstrapAsync(typeId);

        _cache.Set(CatalogBootstrapCacheKey, bootstrap, CatalogBootstrapCacheDuration);
        return bootstrap;
    }

    public async Task<HashSet<int>> GetOrderableCategoryIdsAsync()
    {
        var bootstrap = await GetCatalogBootstrapAsync();
        return bootstrap.OrderableCategoryIds;
    }

    public async Task<(List<CatalogArticleDto> Items, int Total)> GetOrderableArticlesPageAsync(
        int categoryId,
        int page,
        int pageSize,
        string? search = null)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 15;

        var typeId = await GetOrderableArticleTypeIdAsync();
        var skip = (page - 1) * pageSize;
        return await _catalogueRepo.GetOrderableArticlesPageAsync(categoryId, typeId, skip, pageSize, search);
    }

    public async Task<(List<CatalogArticleDto> Items, int Total)> SearchOrderableArticlesPageAsync(
        int page,
        int pageSize,
        string search)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 15;

        if (string.IsNullOrWhiteSpace(search))
            return ([], 0);

        var typeId = await GetOrderableArticleTypeIdAsync();
        var skip = (page - 1) * pageSize;
        return await _catalogueRepo.SearchOrderableArticlesPageAsync(typeId, skip, pageSize, search.Trim());
    }

    // Categories
    public async Task<List<ArticleCategory>> GetArticleCategoriesAsync()
        => await _catalogueRepo.GetAllCategoriesAsync();

    public async Task UpdateCategoryAsync(ArticleCategory category)
    {
        await _catalogueRepo.UpdateCategoryAsync(category);
        _cache.Remove(CatalogBootstrapCacheKey);
    }

    public async Task AddCategoryAsync(ArticleCategory category)
    {
        await _catalogueRepo.AddCategoryAsync(category);
        _cache.Remove(CatalogBootstrapCacheKey);
    }

    public async Task DeleteCategoryAsync(ArticleCategory category)
    {
        await _catalogueRepo.DeleteCategoryAsync(category);
        _cache.Remove(CatalogBootstrapCacheKey);
    }

    // Types
    public async Task<List<ArticleType>> GetArticleTypesAsync()
        => await _catalogueRepo.GetAllTypesAsync();

    public async Task AddTypeAsync(ArticleType type)
        => await _catalogueRepo.AddTypeAsync(type);

    public async Task UpdateTypeAsync(ArticleType type)
        => await _catalogueRepo.UpdateTypeAsync(type);

    public async Task DeleteTypeAsync(ArticleType type)
        => await _catalogueRepo.DeleteTypeAsync(type);

    // Marques
    public async Task<List<ArticleMarque>> GetArticleMarquesAsync()
        => await _catalogueRepo.GetAllMarquesAsync();

    public async Task AddMarqueAsync(ArticleMarque marque)
        => await _catalogueRepo.AddMarqueAsync(marque);

    public async Task UpdateMarqueAsync(ArticleMarque marque)
        => await _catalogueRepo.UpdateMarqueAsync(marque);

    public async Task DeleteMarqueAsync(ArticleMarque marque)
        => await _catalogueRepo.DeleteMarqueAsync(marque);

    // Mesures
    public async Task<List<Mesure>> GetMesuresAsync()
        => await _catalogueRepo.GetAllMesuresAsync();

    public async Task AddMesureAsync(Mesure mesure)
        => await _catalogueRepo.AddMesureAsync(mesure);

    public async Task UpdateMesureAsync(Mesure mesure)
        => await _catalogueRepo.UpdateMesureAsync(mesure);

    public async Task DeleteMesureAsync(Mesure mesure)
        => await _catalogueRepo.DeleteMesureAsync(mesure);

    // Localisations
    public async Task AddLocalisationAsync(Domain.Entities.Administration.Localisation localisation)
        => await _lookupRepo.AddLocalisationAsync(localisation);

    public async Task UpdateLocalisationAsync(Domain.Entities.Administration.Localisation localisation)
        => await _lookupRepo.UpdateLocalisationAsync(localisation);

    public async Task DeleteLocalisationAsync(Domain.Entities.Administration.Localisation localisation)
        => await _lookupRepo.DeleteLocalisationAsync(localisation);

    // Article CRUD
    public async Task<Article?> GetArticleByIdAsync(string id)
        => await _catalogueRepo.GetByArticleIdAsync(id);

    public async Task AddArticleAsync(Article article)
        => await _catalogueRepo.AddAsync(article);

    public async Task UpdateArticleAsync(Article article)
        => await _catalogueRepo.UpdateAsync(article);

    public async Task DeleteArticleAsync(string articleId)
    {
        var article = await _catalogueRepo.GetByArticleIdAsync(articleId);
        if (article != null)
            await _catalogueRepo.DeleteAsync(article);
    }

    public async Task<bool> ArticleExistsAsync(string articleCode)
        => await _catalogueRepo.ArticleExistsAsync(articleCode);

    public async Task<bool> ArticleDescriptionExistsAsync(string description)
        => await _catalogueRepo.ArticleDescriptionExistsAsync(description);

    public async Task CreateArticleWithStocksAsync(Article article, List<int> localisationIds)
    {
        // 1) Create the article
        await _catalogueRepo.AddAsync(article);

        // 2) Create a Stock entry for each localisation (Qte=0, Seuil=0)
        foreach (var locId in localisationIds)
        {
            var stock = new StockEntity
            {
                IdArticle = article.IdArticle,
                IdLocalisation = locId,
                Qte = 0,
                Seuil = 0,
                QteMax = StockDefaults.DefaultQteMax,
                DateSys = DateOnly.FromDateTime(DateTime.Today),
                UserLogin = article.User
            };
            await _stockRepo.AddAsync(stock);
        }
    }

    // Localisation (read)
    public async Task<Domain.Entities.Administration.Localisation?> GetLocalisationByIdAsync(int id)
        => await _lookupRepo.GetLocalisationByIdAsync(id);
}
