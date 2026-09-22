using AppPlusPlus.Domain.Entities.Shared;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Domain.Entities.Finance;
using AppPlusPlus.Domain.Entities.Approvisionnement;

namespace AppPlusPlus.Application.Services.Shared;

public interface ILookupService
{
    Task<List<Money>> GetMoneysAsync();
    Task<List<Caisse>> GetCaissesAsync();
    Task<List<ExpenseSource>> GetExpenseSourcesAsync();
    Task<List<Reduction>> GetReductionsAsync();
}
