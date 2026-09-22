using AppPlusPlus.Domain.Entities.Shared;
using AppPlusPlus.Domain.Entities.Finance;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Domain.Entities.Approvisionnement;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface ILookupRepository
{
    // Status
    Task<List<Status>> GetAllStatusAsync();
    Task<List<StatusCmd>> GetAllStatusCmdAsync();
    Task<List<StatusFactDetail>> GetAllStatusFactDetailAsync();

    // Currency & Money
    Task<List<Currency>> GetAllCurrenciesAsync();
    Task<List<Money>> GetAllMoneysAsync();

    // Localisation
    Task<Localisation?> GetLocalisationByIdAsync(int id);
    Task<List<Localisation>> GetAllLocalisationsAsync();
    Task AddLocalisationAsync(Localisation localisation);
    Task UpdateLocalisationAsync(Localisation localisation);
    Task DeleteLocalisationAsync(Localisation localisation);
    Task<Localisation?> GetDepotLocalisationAsync();

    // Reduction
    Task<List<Reduction>> GetAllReductionsAsync();

    // ExpenseSource
    Task<List<ExpenseSource>> GetAllExpenseSourcesAsync();
}
