using AppPlusPlus.Application.DTOs.Approvisionnement;

namespace AppPlusPlus.Application.Services.Approvisionnement;

public interface IApproQueryService
{
    Task<List<ApproListItemDto>> GetListAsync(
        IReadOnlyList<int> localisationIds,
        CancellationToken cancellationToken = default);

    Task<ApproDetailDto?> GetDetailAsync(int approId, CancellationToken cancellationToken = default);
}
