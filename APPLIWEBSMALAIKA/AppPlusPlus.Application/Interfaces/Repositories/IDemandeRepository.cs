using AppPlusPlus.Domain.Entities.Demandes;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface IDemandeRepository
{
    Task<List<Demande>> GetAllWithDetailsAsync();
    Task<Demande?> GetByIdWithDetailsAsync(int id);
    Task<List<Demande>> GetForUserAsync(bool hasGlobalScope, List<int> userLocIds);
    Task AddAsync(Demande demande);
    Task UpdateAsync(Demande demande);
}
