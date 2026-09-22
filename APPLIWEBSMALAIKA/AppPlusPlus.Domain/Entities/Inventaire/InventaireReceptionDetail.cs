using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Catalogue;

namespace AppPlusPlus.Domain.Entities.Inventaire;

[Table("T_Inventaire_Reception_Detail")]
public class InventaireReceptionDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Id_Reception")]
    public int IdReception { get; set; }

    [Required, MaxLength(100)]
    [Column("Id_Article")]
    public string IdArticle { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantite { get; set; }

    [Column("Prix_Unitaire", TypeName = "decimal(18,2)")]
    public decimal PrixUnitaire { get; set; }

    [Column("Montant_Ligne", TypeName = "decimal(18,2)")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal MontantLigne { get; set; }

    [MaxLength(255)]
    public string? Observation { get; set; }

    [Column("Date_Ligne")]
    public DateTime DateLigne { get; set; } = DateTime.Now;

    [Column("Date_Expiration")]
    public DateOnly? DateExpiration { get; set; }

    [ForeignKey(nameof(IdReception))]
    public InventaireReception? Reception { get; set; }

    [ForeignKey(nameof(IdArticle))]
    public Article? Article { get; set; }
}
