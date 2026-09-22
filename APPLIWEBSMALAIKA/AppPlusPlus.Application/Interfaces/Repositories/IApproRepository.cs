using AppPlusPlus.Domain.Entities.Approvisionnement;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface IApproRepository : IRepository<Appro>
{
    Task<Appro?> GetWithDetailsAsync(int id);
    Task<List<Appro>> GetByDateRangeAsync(DateOnly from, DateOnly to);
    Task<List<Appro>> GetBySupplierAsync(int supplierId);
    Task<List<Appro>> GetByLocalisationAsync(int localisationId);
    Task<List<Appro>> GetByUserAsync(string userLogin);
    Task<List<ApproDetail>> GetDetailsByApproIdAsync(int approId);

    // Transformations
    Task<Transformation?> GetTransformationByIdAsync(int id);
    Task<List<Transformation>> GetTransformationsByLocalisationAsync(int localisationId);
    Task AddTransformationAsync(Transformation transformation);
    Task UpdateTransformationAsync(Transformation transformation);
    Task DeleteTransformationAsync(int id);
}
