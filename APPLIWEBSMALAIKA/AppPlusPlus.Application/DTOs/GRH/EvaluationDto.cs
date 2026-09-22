namespace AppPlusPlus.Application.DTOs.GRH.Evaluation;

public class CreatePerformanceEvaluationDto
{
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public string Period { get; set; } = string.Empty;
    public DateTime EvaluationDate { get; set; }
    public int EvaluatorId { get; set; }
    public string? Comments { get; set; }
    public string? Strengths { get; set; }
    public string? ImprovementAreas { get; set; }
    public string? Goals { get; set; }
}

public class AddEvaluationCriteriaDto
{
    public int EvaluationId { get; set; }
    public string CriteriaName { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public int Rating { get; set; }
    public string? Comments { get; set; }
}

public class PerformanceEvaluationReadDto
{
    public int Id { get; set; }
    public string EvaluationCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Period { get; set; } = string.Empty;
    public DateTime EvaluationDate { get; set; }
    public int OverallRating { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Comments { get; set; }
}
