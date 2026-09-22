using AppPlusPlus.Application.Common;
using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Application.Services.Approvisionnement;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Entities.Approvisionnement;

namespace AppPlusPlus.Application.Services.Approvisionnement;

public class TransformationService : ITransformationService
{
    private readonly IApproRepository _approRepo;
    private readonly IStockDocumentReversal _reversal;
    private readonly ITransformationPostingService _posting;

    public TransformationService(
        IApproRepository approRepo,
        IStockDocumentReversal reversal,
        ITransformationPostingService posting)
    {
        _approRepo = approRepo;
        _reversal = reversal;
        _posting = posting;
    }

    public async Task<List<Transformation>> GetTransformationsByLocalisationsAsync(List<int> localisationIds)
    {
        var results = new List<Transformation>();
        foreach (var locId in localisationIds)
        {
            var transformations = await _approRepo.GetTransformationsByLocalisationAsync(locId);
            results.AddRange(transformations);
        }
        return results;
    }

    public async Task DeleteTransformationAsync(int transformationId)
    {
        var trans = await _approRepo.GetTransformationByIdAsync(transformationId);
        if (trans == null)
            throw new InvalidOperationException("Transformation introuvable.");

        var user = trans.UserLogin ?? "system";
        await _reversal.ReverseTransformationAsync(transformationId, user);
        await _approRepo.DeleteTransformationAsync(transformationId);
    }

    public async Task<Transformation?> GetByIdAsync(int id)
        => await _approRepo.GetTransformationByIdAsync(id);

    public Task<ServiceResult> AddAsync(Transformation transformation)
        => _posting.PostAsync(transformation);

    public async Task<ServiceResult> UpdateAsync(Transformation transformation)
    {
        await _approRepo.UpdateTransformationAsync(transformation);
        return ServiceResult.Ok("Transformation modifiée avec succès.");
    }
}
