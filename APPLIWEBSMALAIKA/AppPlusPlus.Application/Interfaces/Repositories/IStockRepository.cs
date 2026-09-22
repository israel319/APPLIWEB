using AppPlusPlus.Domain.Entities.Stock;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface IStockRepository : IRepository<Stock>
{
    Task<Stock?> GetByArticleAndLocalisationAsync(string articleId, int localisationId);
    Task<List<Stock>> GetByLocalisationAsync(int localisationId);
    Task<List<Stock>> GetByLocalisationIdsAsync(List<int> localisationIds);
    Task<List<Stock>> GetByIdsAsync(IEnumerable<int> stockIds);
    Task<List<Stock>> GetByArticleIdAsync(string articleId);
    Task<List<Stock>> GetLowStockAsync(List<int> localisationIds);
    Task<List<Stock>> GetOutOfStockAsync(List<int> localisationIds);
    Task AddMouvementAsync(MouvementStock mouvement);
    Task UpdateSeuilDirectAsync(int stockId, int seuil);
    Task UpdateQteMaxDirectAsync(int stockId, int qteMax);
    Task<List<Stock>> GetLowStockUnderSeuilAsync(List<int> localisationIds);
    Task<List<MouvementStock>> GetMouvementsByArticleAsync(string articleId, int localisationId);
    Task<List<MouvementStock>> GetMouvementsByDateRangeAsync(DateTime from, DateTime to, List<int> localisationIds);
}
