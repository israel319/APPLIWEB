namespace AppPlusPlus.Domain.Entities.GRH.Evaluation;

/// <summary>
/// Entité représentant un avis d'employé
/// </summary>
public class EmployeeReview
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int ReviewedById { get; set; }
    public int Year { get; set; }
    public string Period { get; set; } = null!;
    public string ReviewType { get; set; } = null!; // Self/Manager/Peer/360
    public string? ReviewText { get; set; }
    public int Rating { get; set; }
    public DateTime ReviewDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public Employee.Employee Employee { get; set; } = null!;
}
