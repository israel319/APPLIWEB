using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Application.DTOs.Catalogue;

namespace AppPlusPlus.Application.Services.Catalogue;

public interface ICatalogueService
{
    // Articles
    Task<List<Article>> GetArticlesAsync();
    Task<int> GetOrderableArticleTypeIdAsync();
    Task<CatalogBootstrapDto> GetCatalogBootstrapAsync();
    Task<HashSet<int>> GetOrderableCategoryIdsAsync();
    Task<(List<CatalogArticleDto> Items, int Total)> GetOrderableArticlesPageAsync(
        int categoryId,
        int page,
        int pageSize,
        string? search = null);
    Task<(List<CatalogArticleDto> Items, int Total)> SearchOrderableArticlesPageAsync(
        int page,
        int pageSize,
        string search);

    // Categories
    Task<List<ArticleCategory>> GetArticleCategoriesAsync();
    Task AddCategoryAsync(ArticleCategory category);
    Task UpdateCategoryAsync(ArticleCategory category);
    Task DeleteCategoryAsync(ArticleCategory category);

    // Types
    Task<List<ArticleType>> GetArticleTypesAsync();
    Task AddTypeAsync(ArticleType type);
    Task UpdateTypeAsync(ArticleType type);
    Task DeleteTypeAsync(ArticleType type);

    // Marques
    Task<List<ArticleMarque>> GetArticleMarquesAsync();
    Task AddMarqueAsync(ArticleMarque marque);
    Task UpdateMarqueAsync(ArticleMarque marque);
    Task DeleteMarqueAsync(ArticleMarque marque);

    // Mesures
    Task<List<Mesure>> GetMesuresAsync();
    Task AddMesureAsync(Mesure mesure);
    Task UpdateMesureAsync(Mesure mesure);
    Task DeleteMesureAsync(Mesure mesure);

    // Localisations
    Task AddLocalisationAsync(Domain.Entities.Administration.Localisation localisation);
    Task UpdateLocalisationAsync(Domain.Entities.Administration.Localisation localisation);
    Task DeleteLocalisationAsync(Domain.Entities.Administration.Localisation localisation);

    // Article CRUD
    Task<Article?> GetArticleByIdAsync(string id);
    Task AddArticleAsync(Article article);
    Task UpdateArticleAsync(Article article);
    Task DeleteArticleAsync(string articleId);

    /// <summary>Returns true if an article with the given code already exists.</summary>
    Task<bool> ArticleExistsAsync(string articleCode);

    /// <summary>Returns true if an article with the given description already exists.</summary>
    Task<bool> ArticleDescriptionExistsAsync(string description);

    /// <summary>
    /// Creates an article and initialises Stock entries (Qte=0, Seuil=0)
    /// for each of the supplied localisation IDs.
    /// </summary>
    Task CreateArticleWithStocksAsync(Article article, List<int> localisationIds);

    // Localisation (read)
    Task<Domain.Entities.Administration.Localisation?> GetLocalisationByIdAsync(int id);
}
