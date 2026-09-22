namespace AppPlusPlus.Domain.Entities.GRH.Absence;

/// <summary>
/// Entité représentant un type d'absence
/// </summary>
public class AbsenceType
{
    public int Id { get; set; }
    public string AbsenceTypeCode { get; set; } = null!;
    public string AbsenceTypeName { get; set; } = null!;
    public string? Description { get; set; }
    public bool RequiresJustification { get; set; }
    public bool IsPaid { get; set; }
    public bool CountsAgainstLeave { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public ICollection<Absence> Absences { get; set; } = new List<Absence>();
}
