using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Domain.Entities.Inventaire;

[Table("T_Inventaire_Magasin")]
public class InventaireMagasin
{
    [Key]
    [Column("Id_Inventaire")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdInventaire { get; set; }

    [Column("Id_Localisation")]
    public int IdLocalisation { get; set; }

    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column("Type_Periode")]
    public int TypePeriode { get; set; }

    [Column("Date_Debut")]
    public DateOnly DateDebut { get; set; }

    [Column("Date_Fin")]
    public DateOnly? DateFin { get; set; }

    [Column("Montant_Initial", TypeName = "decimal(18,2)")]
    public decimal MontantInitial { get; set; }

    [Column("Montant_Receptions", TypeName = "decimal(18,2)")]
    public decimal MontantReceptions { get; set; }

    [Column("Montant_Ventes", TypeName = "decimal(18,2)")]
    public decimal MontantVentes { get; set; }

    [Column("Montant_Theorique", TypeName = "decimal(18,2)")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal MontantTheorique { get; set; }

    public int Statut { get; set; }

    [Column("Cree_Par")]
    [MaxLength(50)]
    public string CreePar { get; set; } = "SYSTEM";

    [Column("Date_Creation")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    [Column("Date_Cloture")]
    public DateTime? DateCloture { get; set; }

    [Column("Cloture_Par")]
    [MaxLength(50)]
    public string? CloturePar { get; set; }

    [ForeignKey(nameof(IdLocalisation))]
    public Localisation? Localisation { get; set; }

    public ICollection<InventaireReception> Receptions { get; set; } = new List<InventaireReception>();
    public ICollection<InventaireVente> Ventes { get; set; } = new List<InventaireVente>();
}
