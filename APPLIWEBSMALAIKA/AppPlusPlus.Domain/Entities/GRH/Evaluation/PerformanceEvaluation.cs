namespace AppPlusPlus.Domain.Entities.GRH.Evaluation;

/// <summary>
/// Entité représentant une évaluation de performance
/// </summary>
public class PerformanceEvaluation
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EvaluationCode { get; set; } = null!;
    public int Year { get; set; }
    public string Period { get; set; } = null!; // Quarterly/Half-Yearly/Annual
    public DateTime EvaluationDate { get; set; }
    public int? EvaluatedById { get; set; }
    public decimal OverallRating { get; set; } // 1-5
    public string? Comments { get; set; }
    public string? Strengths { get; set; }
    public string? AreasForImprovement { get; set; }
    public string? GoalsForNextPeriod { get; set; }
    public string? Status { get; set; } // Draft/Completed/Approved
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee.Employee Employee { get; set; } = null!;
    public ICollection<EvaluationCriteria> Criteria { get; set; } = new List<EvaluationCriteria>();
}
