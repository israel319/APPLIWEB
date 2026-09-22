namespace AppPlusPlus.Application.Services.Stock;

public record StockTransferLine(string ArticleId, decimal Quantity);

public interface IStockTransferService
{
    Task<int> TransferAsync(
        int sourceLocalisationId,
        int destLocalisationId,
        IEnumerable<StockTransferLine> lines,
        string userLogin,
        string reference,
        string? observation,
        string typeDocument,
        int? documentId);
}
