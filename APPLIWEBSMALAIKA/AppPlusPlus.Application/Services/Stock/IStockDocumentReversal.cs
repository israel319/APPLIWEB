namespace AppPlusPlus.Application.Services.Stock;

public interface IStockDocumentReversal
{
    Task ReverseTransformationAsync(int transformationId, string userLogin, CancellationToken cancellationToken = default);
}
