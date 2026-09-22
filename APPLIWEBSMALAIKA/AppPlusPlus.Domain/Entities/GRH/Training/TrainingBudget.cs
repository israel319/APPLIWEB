namespace AppPlusPlus.Domain.Entities.GRH.Training;

/// <summary>
/// Entité représentant le budget de formation d'un département
/// </summary>
public class TrainingBudget
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public int Year { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal AllocatedBudget { get; set; }
    public decimal UsedBudget { get; set; }
    public decimal RemainingBudget { get; set; }
    public string? Status { get; set; } // Active/Exhausted/Archived
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Organization.Department Department { get; set; } = null!;
}
