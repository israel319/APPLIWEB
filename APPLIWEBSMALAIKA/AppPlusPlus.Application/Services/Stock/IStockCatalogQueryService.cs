using AppPlusPlus.Application.DTOs.Stock;
using StockEntity = AppPlusPlus.Domain.Entities.Stock.Stock;

namespace AppPlusPlus.Application.Services.Stock;

public interface IStockCatalogQueryService
{
    Task<StockCatalogQueryResult> GetCatalogAsync(
        IReadOnlyList<int> localisationIds,
        CancellationToken cancellationToken = default);
}

public sealed class StockCatalogQueryResult
{
    public List<StockCatalogArticleDto> Articles { get; init; } = new();
    public List<StockEntity> Stocks { get; init; } = new();
}
