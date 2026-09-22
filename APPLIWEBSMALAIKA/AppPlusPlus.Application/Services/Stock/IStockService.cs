using AppPlusPlus.Application.DTOs.Stock;
using StockEntity = AppPlusPlus.Domain.Entities.Stock.Stock;

namespace AppPlusPlus.Application.Services.Stock;

public interface IStockService
{
    Task<List<StockEntity>> GetStocksByLocalisationsAsync(List<int> localisationIds);
    Task<List<StockEntity>> GetAllStocksAsync();
    Task<List<StockEntity>> GetLowStockArticlesAsync(List<int> localisationIds);

    Task DeleteArticleWithStocksAsync(string articleId);

    /// <summary>Supprime un article sans stock. Refuse si quantité > 0.</summary>
    Task DeleteArticleSafelyAsync(string articleId);
    Task<List<StockEntity>> GetStocksByArticleAsync(string articleId);

    /// <summary>
    /// Returns a flat list of stock + article info for a single localisation,
    /// combining Stock.Qte/Seuil with Article.Description.
    /// </summary>
    Task<List<StockArticleDto>> GetStockArticleViewAsync(int localisationId);

    Task UpdateSeuilAsync(int stockId, int seuil);
    Task UpdateQteMaxAsync(int stockId, int qteMax);
}
