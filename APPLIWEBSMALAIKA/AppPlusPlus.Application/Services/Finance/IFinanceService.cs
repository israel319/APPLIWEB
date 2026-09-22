using AppPlusPlus.Domain.Entities.Finance;

namespace AppPlusPlus.Application.Services.Finance;

public interface IFinanceService
{
    Task<List<Taux>> GetTauxAsync();
    Task<List<Periode>> GetPeriodesAsync();
    Task<List<Versement>> GetVersementsAsync();

    Task<Taux?> GetTauxByIdAsync(int id);
    Task AddTauxAsync(Taux taux);
    Task UpdateTauxAsync(Taux taux);
    Task<Periode?> GetActivePeriodeAsync();
    Task<Periode?> GetPeriodeByIdAsync(int id);
    Task AddPeriodeAsync(Periode periode);
    Task UpdatePeriodeAsync(Periode periode);
}
