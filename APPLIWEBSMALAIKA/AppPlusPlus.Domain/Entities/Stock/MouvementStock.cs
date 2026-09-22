using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Domain.Entities.Stock;

/// <summary>
/// Tracabilite des mouvements de stock (entrees, sorties, transferts, ajustements)
/// </summary>
[Table("T_Mouvement_Stock")]
public class MouvementStock
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    // Article et Localisation
    [Required]
    [MaxLength(100)]
    [Column("Id_Article")]
    public string IdArticle { get; set; } = string.Empty;

    [Column("Id_Localisation")]
    public int IdLocalisation { get; set; }

    // Type de mouvement
    [Required]
    [MaxLength(20)]
    [Column("Type_Mouvement")]
    public string TypeMouvement { get; set; } = string.Empty; // ENTREE / SORTIE / TRANSFERT / AJUSTEMENT

    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantite { get; set; }

    [Column("Qte_Avant", TypeName = "decimal(18,2)")]
    public decimal? QteAvant { get; set; }

    [Column("Qte_Apres", TypeName = "decimal(18,2)")]
    public decimal? QteApres { get; set; }

    [Column("Date_Mouvement")]
    public DateTime DateMouvement { get; set; } = DateTime.Now;

    // Reference au document source
    [Required]
    [MaxLength(50)]
    [Column("Type_Document")]
    public string TypeDocument { get; set; } = string.Empty; // APPRO / FACTURE / LIVRAISON / INVENTAIRE / TRANSFERT

    [Column("Id_Document")]
    public int? IdDocument { get; set; }

    [Column("Id_Document_Detail")]
    public int? IdDocumentDetail { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    // Source et destination (pour transferts)
    [Column("Id_Localisation_Source")]
    public int? IdLocalisationSource { get; set; }

    [Column("Id_Localisation_Dest")]
    public int? IdLocalisationDest { get; set; }

    // Valeurs
    [Column("Prix_Unitaire", TypeName = "decimal(18,2)")]
    public decimal? PrixUnitaire { get; set; }

    [Column("Valeur_Totale", TypeName = "decimal(18,2)")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal? ValeurTotale { get; set; }

    // Audit
    [MaxLength(255)]
    public string? Observation { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("Cree_Par")]
    public string CreePar { get; set; } = string.Empty;

    [Column("Date_Creation")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    [Column("Date_Expiration")]
    public DateOnly? DateExpiration { get; set; }

    [MaxLength(50)]
    [Column("Valide_Par")]
    public string? ValidePar { get; set; }

    [Column("Date_Validation")]
    public DateTime? DateValidation { get; set; }

    public bool Annule { get; set; } = false;

    // Navigation properties
    [ForeignKey(nameof(IdArticle))]
    public Article? Article { get; set; }

    [ForeignKey(nameof(IdLocalisation))]
    public Localisation? Localisation { get; set; }

    [ForeignKey(nameof(IdLocalisationSource))]
    public Localisation? LocalisationSource { get; set; }

    [ForeignKey(nameof(IdLocalisationDest))]
    public Localisation? LocalisationDest { get; set; }
}
