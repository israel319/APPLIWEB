using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Application.DTOs.Catalogue;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface ICatalogueRepository : IRepository<Article>
{
    Task<Article?> GetByArticleIdAsync(string articleId);
    Task<bool> ArticleExistsAsync(string articleCode);
    Task<bool> ArticleDescriptionExistsAsync(string description);
    Task<List<Article>> GetArticlesWithStockAsync(List<int> localisationIds);
    Task<List<ArticleType>> GetAllTypesAsync();
    Task<List<ArticleMarque>> GetAllMarquesAsync();
    Task<List<ArticleCategory>> GetAllCategoriesAsync();
    Task<List<Mesure>> GetAllMesuresAsync();

    Task<CatalogBootstrapDto> GetCatalogBootstrapAsync(int orderableTypeId);
    Task<HashSet<int>> GetCategoryIdsWithOrderableArticlesAsync(int orderableTypeId);
    Task<Dictionary<int, List<string>>> GetPreviewImagesByCategoryAsync(int orderableTypeId, int maxPerCategory = 3);
    Task<(List<CatalogArticleDto> Items, int Total)> GetOrderableArticlesPageAsync(
        int categoryId,
        int orderableTypeId,
        int skip,
        int take,
        string? search = null);
    Task<(List<CatalogArticleDto> Items, int Total)> SearchOrderableArticlesPageAsync(
        int orderableTypeId,
        int skip,
        int take,
        string search);

    // CRUD for reference data
    Task AddCategoryAsync(ArticleCategory category);
    Task UpdateCategoryAsync(ArticleCategory category);
    Task DeleteCategoryAsync(ArticleCategory category);
    Task AddTypeAsync(ArticleType type);
    Task UpdateTypeAsync(ArticleType type);
    Task DeleteTypeAsync(ArticleType type);
    Task AddMarqueAsync(ArticleMarque marque);
    Task UpdateMarqueAsync(ArticleMarque marque);
    Task DeleteMarqueAsync(ArticleMarque marque);
    Task AddMesureAsync(Mesure mesure);
    Task UpdateMesureAsync(Mesure mesure);
    Task DeleteMesureAsync(Mesure mesure);
}
