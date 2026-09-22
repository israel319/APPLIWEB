using AppPlusPlus.Application.DTOs.Stock;
using AppPlusPlus.Application.Interfaces.Repositories;
using StockEntity = AppPlusPlus.Domain.Entities.Stock.Stock;

namespace AppPlusPlus.Application.Services.Stock;

public class StockService : IStockService
{
    private readonly IStockRepository _stockRepo;
    private readonly ICatalogueRepository _catalogueRepo;

    public StockService(IStockRepository stockRepo, ICatalogueRepository catalogueRepo)
    {
        _stockRepo = stockRepo;
        _catalogueRepo = catalogueRepo;
    }

    public async Task<List<StockEntity>> GetStocksByLocalisationsAsync(List<int> localisationIds)
        => await _stockRepo.GetByLocalisationIdsAsync(localisationIds);

    public async Task<List<StockEntity>> GetAllStocksAsync()
        => await _stockRepo.GetAllAsync();

    public async Task<List<StockEntity>> GetLowStockArticlesAsync(List<int> localisationIds)
        => await _stockRepo.GetLowStockAsync(localisationIds);

    public async Task<List<StockEntity>> GetStocksByArticleAsync(string articleId)
        => await _stockRepo.GetByArticleIdAsync(articleId);

    public async Task DeleteArticleWithStocksAsync(string articleId)
    {
        var stocks = await _stockRepo.GetByArticleIdAsync(articleId);
        foreach (var stock in stocks)
            await _stockRepo.DeleteAsync(stock);

        var article = await _catalogueRepo.GetByArticleIdAsync(articleId);
        if (article != null)
            await _catalogueRepo.DeleteAsync(article);
    }

    public async Task DeleteArticleSafelyAsync(string articleId)
    {
        var stocks = await _stockRepo.GetByArticleIdAsync(articleId);
        if (stocks.Any(s => s.Qte > 0))
            throw new InvalidOperationException(
                "Impossible de supprimer un article avec du stock. Annulez ou ajustez d'abord les mouvements.");

        await DeleteArticleWithStocksAsync(articleId);
    }

    public async Task<List<StockArticleDto>> GetStockArticleViewAsync(int localisationId)
    {
        // GetByLocalisationAsync includes Article navigation
        var stocks = await _stockRepo.GetByLocalisationAsync(localisationId);

        return stocks
            .Select(s => new StockArticleDto
            {
                IdArticle = s.IdArticle,
                Description = s.Article?.Description ?? s.IdArticle,
                Qte = s.Qte,
                Seuil = s.Seuil,
                QteMax = s.QteMax,
                Price = s.Article?.Price ?? 0
            })
            .OrderBy(dto => dto.Description)
            .ToList();
    }

    public async Task UpdateSeuilAsync(int stockId, int seuil)
    {
        await _stockRepo.UpdateSeuilDirectAsync(stockId, seuil);
    }

    public async Task UpdateQteMaxAsync(int stockId, int qteMax)
    {
        await _stockRepo.UpdateQteMaxDirectAsync(stockId, qteMax);
    }
}
