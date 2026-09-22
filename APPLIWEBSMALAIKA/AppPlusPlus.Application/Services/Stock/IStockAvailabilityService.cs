namespace AppPlusPlus.Application.Services.Stock;

public interface IStockAvailabilityService
{
    Task<decimal> GetAvailableAsync(string articleId, int localisationId, CancellationToken cancellationToken = default);

    Task<Dictionary<(string ArticleId, int LocalisationId), decimal>> GetAvailableBatchAsync(
        IEnumerable<(string ArticleId, int LocalisationId)> keys,
        CancellationToken cancellationToken = default);

    /// <summary>Stock max transférable depuis une commande (min stock physique, restant commande).</summary>
    Task<decimal> GetTransferableFromCommandAsync(
        string articleId,
        int sourceLocalisationId,
        decimal commandRemaining,
        CancellationToken cancellationToken = default);
}
