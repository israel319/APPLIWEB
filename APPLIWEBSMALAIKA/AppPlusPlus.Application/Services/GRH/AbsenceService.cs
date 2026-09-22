using AppPlusPlus.Domain.Entities.GRH.Absence;
using AppPlusPlus.Application.Repositories.GRH;

namespace AppPlusPlus.Application.Services.GRH;

public class AbsenceService : IAbsenceService
{
    private readonly IAbsenceRepository _absenceRepository;

    public AbsenceService(IAbsenceRepository absenceRepository)
    {
        _absenceRepository = absenceRepository;
    }

    public async Task<Absence> CreateAbsenceAsync(int employeeId, int absenceTypeId, DateTime absenceDate, int duration, string reason)
    {
        var absence = new Absence
        {
            EmployeeId = employeeId,
            AbsenceTypeId = absenceTypeId,
            AbsenceNumber = $"ABS-{DateTime.UtcNow:yyyyMMddHHmmss}",
            AbsenceDate = absenceDate,
            Duration = duration,
            Reason = reason,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _absenceRepository.AddAsync(absence);
        return absence;
    }

    public async Task<Absence> GetAbsenceAsync(int id)
    {
        return await _absenceRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Absence>> GetAllAbsencesAsync()
    {
        return await _absenceRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Absence>> GetEmployeeAbsencesAsync(int employeeId)
    {
        return await _absenceRepository.GetByEmployeeAsync(employeeId);
    }

    public async Task<IEnumerable<Absence>> GetAbsencesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _absenceRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task ApproveAbsenceAsync(int absenceId)
    {
        var absence = await _absenceRepository.GetByIdAsync(absenceId);
        absence.Status = "Approved";
        await _absenceRepository.UpdateAsync(absence);
    }

    public async Task RejectAbsenceAsync(int absenceId)
    {
        var absence = await _absenceRepository.GetByIdAsync(absenceId);
        absence.Status = "Rejected";
        await _absenceRepository.UpdateAsync(absence);
    }

    public async Task<int> GetTotalAbsenceDaysAsync(int employeeId, int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31);
        var absences = await _absenceRepository.GetByDateRangeAsync(startDate, endDate);
        return absences.Where(a => a.EmployeeId == employeeId).Sum(a => a.Duration);
    }

    public async Task UpdateAbsenceAsync(Absence absence)
    {
        absence.UpdatedAt = DateTime.UtcNow;
        await _absenceRepository.UpdateAsync(absence);
    }

    public async Task DeleteAbsenceAsync(int id)
    {
        await _absenceRepository.DeleteAsync(id);
    }
}
