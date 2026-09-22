using AppPlusPlus.Domain.Entities.GRH.Training;

namespace AppPlusPlus.Application.Services.GRH;

/// <summary>
/// Service pour gérer la formation
/// </summary>
public interface ITrainingService
{
    Task<TrainingProgram> CreateTrainingProgramAsync(string programCode, string programName, string category);
    Task<TrainingSession> CreateTrainingSessionAsync(int programId, DateTime startDate, DateTime endDate, string location);
    Task<EmployeeTraining> EnrollEmployeeAsync(int employeeId, int sessionId);
    Task<IEnumerable<TrainingProgram>> GetAllProgramsAsync();
    Task<IEnumerable<TrainingSession>> GetProgramSessionsAsync(int programId);
    Task<IEnumerable<EmployeeTraining>> GetEmployeeTrainingsAsync(int employeeId);
    Task CompleteTrainingAsync(int employeeTrainingId, bool certified);
    Task<int> GetParticipantCountAsync(int sessionId);
    Task<TrainingBudget> GetDepartmentTrainingBudgetAsync(int departmentId, int year);
}
