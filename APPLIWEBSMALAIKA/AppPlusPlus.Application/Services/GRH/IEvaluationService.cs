using AppPlusPlus.Domain.Entities.GRH.Evaluation;

namespace AppPlusPlus.Application.Services.GRH;

/// <summary>
/// Service pour gérer l'évaluation
/// </summary>
public interface IEvaluationService
{
    Task<PerformanceEvaluation> CreateEvaluationAsync(int employeeId, int year, string period);
    Task<PerformanceEvaluation> GetEvaluationAsync(int id);
    Task<IEnumerable<PerformanceEvaluation>> GetEmployeeEvaluationsAsync(int employeeId);
    Task<IEnumerable<PerformanceEvaluation>> GetYearEvaluationsAsync(int year);
    Task AddEvaluationCriteriaAsync(int evaluationId, string criteriaName, int rating);
    Task UpdateEvaluationAsync(PerformanceEvaluation evaluation);
    Task CompleteEvaluationAsync(int evaluationId);
    Task<decimal> CalculateOverallRatingAsync(int evaluationId);
    Task<IEnumerable<PerformanceEvaluation>> GetPendingEvaluationsAsync();
}
