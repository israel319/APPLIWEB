namespace AppPlusPlus.Domain.Entities.GRH.Organization;

/// <summary>
/// Entité représentant une relation hiérarchique
/// </summary>
public class Hierarchy
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int SupervisedById { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
}
