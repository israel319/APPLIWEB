using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Domain.Entities.Finance;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Application.Services.Approvisionnement;

public class ApproService : IApproService
{
    private readonly IApproRepository _approRepo;
    private readonly IFinanceRepository _financeRepo;

    public ApproService(IApproRepository approRepo, IFinanceRepository financeRepo)
    {
        _approRepo = approRepo;
        _financeRepo = financeRepo;
    }

    public async Task<List<Appro>> GetApprosByLocalisationsAsync(List<int> localisationIds)
    {
        // Phase 3: Aggregate results from multiple localisations
        var results = new List<Appro>();
        foreach (var locId in localisationIds)
        {
            var appros = await _approRepo.GetByLocalisationAsync(locId);
            results.AddRange(appros);
        }
        return results;
    }

    public async Task DeleteApproAsync(int approId)
    {
        var appro = await _approRepo.GetByIdAsync(approId);
        if (appro != null)
            await _approRepo.DeleteAsync(appro);
    }

    public async Task<ApproExpense?> GetApproExpenseByIdAsync(int id)
        => await _financeRepo.GetApproExpenseByIdAsync(id);

    public async Task AddApproExpenseAsync(ApproExpense expense)
        => await _financeRepo.AddApproExpenseAsync(expense);

    public async Task UpdateApproExpenseAsync(ApproExpense expense)
        => await _financeRepo.UpdateApproExpenseAsync(expense);
}
