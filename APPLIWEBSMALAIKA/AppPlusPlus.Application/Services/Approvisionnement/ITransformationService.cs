using AppPlusPlus.Application.Common;
using AppPlusPlus.Domain.Entities.Approvisionnement;

namespace AppPlusPlus.Application.Services.Approvisionnement;

public interface ITransformationService
{
    Task<List<Transformation>> GetTransformationsByLocalisationsAsync(List<int> localisationIds);
    Task DeleteTransformationAsync(int transformationId);

    Task<Transformation?> GetByIdAsync(int id);
    Task<ServiceResult> AddAsync(Transformation transformation);
    Task<ServiceResult> UpdateAsync(Transformation transformation);
}
