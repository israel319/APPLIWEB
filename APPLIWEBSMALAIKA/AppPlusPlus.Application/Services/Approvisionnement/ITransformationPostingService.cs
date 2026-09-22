using AppPlusPlus.Application.Common;
using AppPlusPlus.Domain.Entities.Approvisionnement;

namespace AppPlusPlus.Application.Services.Approvisionnement;

public interface ITransformationPostingService
{
    Task<ServiceResult> PostAsync(Transformation transformation, CancellationToken cancellationToken = default);
}
