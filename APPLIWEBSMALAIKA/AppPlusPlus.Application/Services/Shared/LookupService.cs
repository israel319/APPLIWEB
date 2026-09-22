using AppPlusPlus.Domain.Entities.Shared;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Domain.Entities.Finance;
using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Application.Services.Shared;

public class LookupService : ILookupService
{
    private readonly ILookupRepository _lookupRepo;
    private readonly IFinanceRepository _financeRepo;

    public LookupService(ILookupRepository lookupRepo, IFinanceRepository financeRepo)
    {
        _lookupRepo = lookupRepo;
        _financeRepo = financeRepo;
    }

    public async Task<List<Money>> GetMoneysAsync()
        => await _lookupRepo.GetAllMoneysAsync();

    public async Task<List<Caisse>> GetCaissesAsync()
        => await _financeRepo.GetAllCaissesAsync();

    public async Task<List<ExpenseSource>> GetExpenseSourcesAsync()
        => await _lookupRepo.GetAllExpenseSourcesAsync();

    public async Task<List<Reduction>> GetReductionsAsync()
        => await _lookupRepo.GetAllReductionsAsync();
}
