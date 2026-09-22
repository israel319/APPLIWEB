using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Vente;

namespace AppPlusPlus.Domain.Entities.Prestations;

/// <summary>Prestation immatérielle du catalogue (équivalent article pour les services).</summary>
[Table("T_Service_Catalog")]
public class ServiceCatalogItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Detail { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int MoneyId { get; set; }

    /// <summary>0=Heure, 1=Forfait, 2=Unité</summary>
    public int UnitType { get; set; }

    [MaxLength(80)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime DateSys { get; set; } = DateTime.Now;

    public DateTime? DateEditing { get; set; }

    [Required, MaxLength(50)]
    [Column("User")]
    public string User { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Cumputer { get; set; } = string.Empty;

    [ForeignKey(nameof(MoneyId))]
    public Money? Money { get; set; }
}
