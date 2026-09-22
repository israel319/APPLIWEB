using AppPlusPlus.Domain.Entities.GRH.Evaluation;
using AppPlusPlus.Application.Repositories.GRH;

namespace AppPlusPlus.Application.Services.GRH;

public class EvaluationService : IEvaluationService
{
    private readonly IEvaluationRepository _evaluationRepository;

    public EvaluationService(IEvaluationRepository evaluationRepository)
    {
        _evaluationRepository = evaluationRepository;
    }

    public async Task<PerformanceEvaluation> CreateEvaluationAsync(int employeeId, int year, string period)
    {
        var evaluation = new PerformanceEvaluation
        {
            EmployeeId = employeeId,
            EvaluationCode = $"EVAL-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Year = year,
            Period = period,
            EvaluationDate = DateTime.UtcNow,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow
        };

        await _evaluationRepository.AddAsync(evaluation);
        return evaluation;
    }

    public async Task<PerformanceEvaluation> GetEvaluationAsync(int id)
    {
        return await _evaluationRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<PerformanceEvaluation>> GetEmployeeEvaluationsAsync(int employeeId)
    {
        return await _evaluationRepository.GetByEmployeeAsync(employeeId);
    }

    public async Task<IEnumerable<PerformanceEvaluation>> GetYearEvaluationsAsync(int year)
    {
        return await _evaluationRepository.GetByYearAsync(year);
    }

    public async Task AddEvaluationCriteriaAsync(int evaluationId, string criteriaName, int rating)
    {
        throw new NotImplementedException("Implement with IEvaluationCriteriaRepository");
    }

    public async Task UpdateEvaluationAsync(PerformanceEvaluation evaluation)
    {
        evaluation.UpdatedAt = DateTime.UtcNow;
        await _evaluationRepository.UpdateAsync(evaluation);
    }

    public async Task CompleteEvaluationAsync(int evaluationId)
    {
        var evaluation = await _evaluationRepository.GetByIdAsync(evaluationId);
        evaluation.Status = "Completed";
        await _evaluationRepository.UpdateAsync(evaluation);
    }

    public async Task<decimal> CalculateOverallRatingAsync(int evaluationId)
    {
        throw new NotImplementedException("Implement with IEvaluationCriteriaRepository");
    }

    public async Task<IEnumerable<PerformanceEvaluation>> GetPendingEvaluationsAsync()
    {
        return await _evaluationRepository.GetByStatusAsync("Draft");
    }
}
