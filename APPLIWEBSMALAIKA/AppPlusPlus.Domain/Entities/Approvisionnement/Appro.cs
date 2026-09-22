using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Fournisseurs;
using AppPlusPlus.Domain.Entities.CommandesInternes;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Domain.Entities.Approvisionnement;

[Table("T_Appros")]
public class Appro : IMonetaryRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Column("Id_Article")]
    public string IdArticle { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Qte { get; set; }

    [Column("PA", TypeName = "decimal(18,2)")]
    public decimal? PA { get; set; }

    [Column("PV", TypeName = "decimal(18,2)")]
    public decimal? PV { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Ben { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? BenTotal { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? Taux { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Montant { get; set; }

    [Column("Id_Monais")]
    public int? MoneyId { get; set; }

    [Column("Montant_Apres_Conversion", TypeName = "decimal(18,2)")]
    public decimal? MontantApresConversion { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public DateTime DateSys { get; set; }

    [Required, MaxLength(50)]
    [Column("User")]
    public string User { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Column("Cumputer")]
    public string Cumputer { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Commentaire { get; set; } = string.Empty;

    public int? SupplierId { get; set; }

    public int? LocalisationId { get; set; }

    public int? StatusId { get; set; }

    // FK vers Commande fournisseur (optionnel)
    [Column("Id_Cmd")]
    public int? IdCmd { get; set; }

    /// <summary>Ligne de commande interne liée (0 = approvisionnement direct sans commande).</summary>
    [Column("Id_Cmd_Detail")]
    public int IdCmdDetail { get; set; }

    [MaxLength(50)]
    public string? Reference { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }

    [ForeignKey(nameof(IdCmd))]
    public Cmd? Cmd { get; set; }

    // Collection de details (lignes d'approvisionnement)
    public ICollection<ApproDetail> Details { get; set; } = new List<ApproDetail>();
}
