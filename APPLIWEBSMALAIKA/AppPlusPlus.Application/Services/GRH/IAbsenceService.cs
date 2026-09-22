using AppPlusPlus.Domain.Entities.GRH.Absence;

namespace AppPlusPlus.Application.Services.GRH;

/// <summary>
/// Service pour gérer les absences
/// </summary>
public interface IAbsenceService
{
    Task<Absence> CreateAbsenceAsync(int employeeId, int absenceTypeId, DateTime absenceDate, int duration, string reason);
    Task<Absence> GetAbsenceAsync(int id);
    Task<IEnumerable<Absence>> GetAllAbsencesAsync();
    Task<IEnumerable<Absence>> GetEmployeeAbsencesAsync(int employeeId);
    Task<IEnumerable<Absence>> GetAbsencesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task ApproveAbsenceAsync(int absenceId);
    Task RejectAbsenceAsync(int absenceId);
    Task<int> GetTotalAbsenceDaysAsync(int employeeId, int year);
    Task UpdateAbsenceAsync(Absence absence);
    Task DeleteAbsenceAsync(int id);
}
