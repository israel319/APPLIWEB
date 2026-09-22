namespace AppPlusPlus.Domain.Entities.GRH.Evaluation;

/// <summary>
/// Entité représentant les critères d'évaluation
/// </summary>
public class EvaluationCriteria
{
    public int Id { get; set; }
    public int PerformanceEvaluationId { get; set; }
    public string CriteriaName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? Weight { get; set; } // %
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public PerformanceEvaluation PerformanceEvaluation { get; set; } = null!;
}
