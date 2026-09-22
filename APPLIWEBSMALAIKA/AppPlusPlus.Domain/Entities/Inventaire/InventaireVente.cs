using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Inventaire;

[Table("T_Inventaire_Vente")]
public class InventaireVente
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Id_Inventaire")]
    public int IdInventaire { get; set; }

    [Column("Date_Vente")]
    public DateOnly DateVente { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Montant { get; set; }

    [Column("Montant_Ventes", TypeName = "decimal(18,2)")]
    public decimal MontantVentes { get; set; }

    [Column("Montant_Depenses", TypeName = "decimal(18,2)")]
    public decimal MontantDepenses { get; set; }

    [MaxLength(255)]
    public string? Observation { get; set; }

    [Column("Cree_Par")]
    [MaxLength(50)]
    public string CreePar { get; set; } = "SYSTEM";

    [Column("Date_Creation")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    [ForeignKey(nameof(IdInventaire))]
    public InventaireMagasin? Inventaire { get; set; }
}
