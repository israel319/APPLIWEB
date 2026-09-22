using AppPlusPlus.Domain.Entities.GRH.Training;
using AppPlusPlus.Application.Repositories.GRH;

namespace AppPlusPlus.Application.Services.GRH;

public class TrainingService : ITrainingService
{
    private readonly ITrainingRepository _trainingRepository;

    public TrainingService(ITrainingRepository trainingRepository)
    {
        _trainingRepository = trainingRepository;
    }

    public async Task<TrainingProgram> CreateTrainingProgramAsync(string programCode, string programName, string category)
    {
        var program = new TrainingProgram
        {
            ProgramCode = programCode,
            ProgramName = programName,
            Category = category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _trainingRepository.AddProgramAsync(program);
        return program;
    }

    public async Task<TrainingSession> CreateTrainingSessionAsync(int programId, DateTime startDate, DateTime endDate, string location)
    {
        var session = new TrainingSession
        {
            TrainingProgramId = programId,
            SessionCode = $"TS-{DateTime.UtcNow:yyyyMMddHHmmss}",
            StartDate = startDate,
            EndDate = endDate,
            Location = location,
            Status = "Planned",
            CreatedAt = DateTime.UtcNow
        };

        await _trainingRepository.AddSessionAsync(session);
        return session;
    }

    public async Task<EmployeeTraining> EnrollEmployeeAsync(int employeeId, int sessionId)
    {
        var employeeTraining = new EmployeeTraining
        {
            EmployeeId = employeeId,
            TrainingSessionId = sessionId,
            Status = "Enrolled",
            CreatedAt = DateTime.UtcNow
        };

        await _trainingRepository.AddEmployeeTrainingAsync(employeeTraining);
        return employeeTraining;
    }

    public async Task<IEnumerable<TrainingProgram>> GetAllProgramsAsync()
    {
        return await _trainingRepository.GetAllProgramsAsync();
    }

    public async Task<IEnumerable<TrainingSession>> GetProgramSessionsAsync(int programId)
    {
        return await _trainingRepository.GetSessionsByProgramAsync(programId);
    }

    public async Task<IEnumerable<EmployeeTraining>> GetEmployeeTrainingsAsync(int employeeId)
    {
        return await _trainingRepository.GetByEmployeeAsync(employeeId);
    }

    public async Task CompleteTrainingAsync(int employeeTrainingId, bool certified)
    {
        var employeeTraining = await _trainingRepository.GetEmployeeTrainingByIdAsync(employeeTrainingId);
        employeeTraining.Status = "Completed";
        employeeTraining.Certified = certified;
    }

    public async Task<int> GetParticipantCountAsync(int sessionId)
    {
        var participants = await _trainingRepository.GetBySessionAsync(sessionId);
        return participants.Count();
    }

    public async Task<TrainingBudget> GetDepartmentTrainingBudgetAsync(int departmentId, int year)
    {
        throw new NotImplementedException("Implement with ITrainingBudgetRepository");
    }
}
