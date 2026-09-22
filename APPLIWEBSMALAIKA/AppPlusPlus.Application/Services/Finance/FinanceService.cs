using AppPlusPlus.Domain.Entities.Finance;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Application.Services.Finance;

public class FinanceService : IFinanceService
{
    private readonly IFinanceRepository _financeRepo;

    public FinanceService(IFinanceRepository financeRepo)
    {
        _financeRepo = financeRepo;
    }

    public async Task<List<Taux>> GetTauxAsync()
        => await _financeRepo.GetAllTauxAsync();

    public async Task<List<Periode>> GetPeriodesAsync()
        => await _financeRepo.GetAllPeriodesAsync();

    public async Task<List<Versement>> GetVersementsAsync()
    {
        // Phase 3: Add proper date range or localisation filtering
        // For now return versements for today as a placeholder
        return await _financeRepo.GetVersementsByDateAsync(DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task<Taux?> GetTauxByIdAsync(int id)
        => await _financeRepo.GetTauxByIdAsync(id);

    public async Task AddTauxAsync(Taux taux)
        => await _financeRepo.AddTauxAsync(taux);

    public async Task UpdateTauxAsync(Taux taux)
        => await _financeRepo.UpdateTauxAsync(taux);

    public async Task<Periode?> GetActivePeriodeAsync()
        => await _financeRepo.GetActivePeriodeAsync();

    public async Task<Periode?> GetPeriodeByIdAsync(int id)
        => await _financeRepo.GetPeriodeByIdAsync(id);

    public async Task AddPeriodeAsync(Periode periode)
        => await _financeRepo.AddPeriodeAsync(periode);

    public async Task UpdatePeriodeAsync(Periode periode)
        => await _financeRepo.UpdatePeriodeAsync(periode);
}
