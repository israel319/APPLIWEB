using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Inventaire;

[Table("T_Inventaire_Reception")]
public class InventaireReception
{
    [Key]
    [Column("Id_Reception")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdReception { get; set; }

    [Column("Id_Inventaire")]
    public int IdInventaire { get; set; }

    [Column("Numero_Reception")]
    [MaxLength(30)]
    public string? NumeroReception { get; set; }

    [MaxLength(50)]
    public string? Reference { get; set; }

    public int Statut { get; set; }

    [Column("Montant_Total", TypeName = "decimal(18,2)")]
    public decimal MontantTotal { get; set; }

    [MaxLength(255)]
    public string? Observation { get; set; }

    [Column("Cree_Par")]
    [MaxLength(50)]
    public string CreePar { get; set; } = "SYSTEM";

    [Column("Date_Creation")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    [Column("Date_Jour")]
    public DateOnly DateJour { get; set; }

    [Column("Date_Cloture")]
    public DateTime? DateCloture { get; set; }

    [Column("Cloture_Par")]
    [MaxLength(50)]
    public string? CloturePar { get; set; }

    [ForeignKey(nameof(IdInventaire))]
    public InventaireMagasin? Inventaire { get; set; }

    public ICollection<InventaireReceptionDetail> Details { get; set; } = new List<InventaireReceptionDetail>();
}
