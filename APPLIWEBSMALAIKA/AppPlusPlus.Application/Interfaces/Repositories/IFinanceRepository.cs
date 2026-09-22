using AppPlusPlus.Domain.Entities.Finance;
using AppPlusPlus.Domain.Entities.Approvisionnement;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface IFinanceRepository
{
    // Caisse
    Task<Caisse?> GetCaisseByIdAsync(int caisseId);
    Task<List<Caisse>> GetAllCaissesAsync();
    Task AddCaisseAsync(Caisse caisse);
    Task UpdateCaisseAsync(Caisse caisse);

    // Taux
    Task<Taux?> GetCurrentTauxAsync();
    Task<Taux?> GetTauxByIdAsync(int id);
    Task<List<Taux>> GetAllTauxAsync();
    Task<List<Taux>> GetTauxByDateRangeAsync(DateOnly from, DateOnly to);
    Task AddTauxAsync(Taux taux);
    Task UpdateTauxAsync(Taux taux);

    // Periode
    Task<Periode?> GetActivePeriodeAsync();
    Task<Periode?> GetPeriodeByIdAsync(int id);
    Task<List<Periode>> GetAllPeriodesAsync();
    Task AddPeriodeAsync(Periode periode);
    Task UpdatePeriodeAsync(Periode periode);

    // Versement
    Task<bool> VersementExistsAsync(DateOnly date, int localisationId);
    Task<List<Versement>> GetVersementsByDateAsync(DateOnly date);
    Task<List<Versement>> GetVersementsByLocalisationAsync(int localisationId);
    Task<List<Versement>> GetVersementsByDateRangeAsync(DateOnly from, DateOnly to);
    Task AddVersementAsync(Versement versement);

    // ApproExpense
    Task<ApproExpense?> GetApproExpenseByIdAsync(int id);
    Task<List<ApproExpense>> GetApproExpensesAsync();
    Task<List<ApproExpense>> GetApproExpensesByCaisseAsync(int caisseId);
    Task AddApproExpenseAsync(ApproExpense expense);
    Task UpdateApproExpenseAsync(ApproExpense expense);

    // UserCaisse
    Task<List<UserCaisse>> GetUserCaissesAsync(string userLogin);
}
