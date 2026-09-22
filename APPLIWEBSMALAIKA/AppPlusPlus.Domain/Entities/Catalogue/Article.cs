using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Catalogue;

/// <summary>
/// Correspond a la table [dbo].[T_Arts] -- Articles.
/// </summary>
[Table("T_Arts")]
public class Article
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Key]
    [Required, MaxLength(100)]
    [Column("Id_Article")]
    public string IdArticle { get; set; } = string.Empty;

    [Required]
    [Column("Id_Marque")]
    public int IdMarque { get; set; }

    [Required]
    [Column("Id_Type")]
    public int IdType { get; set; }

    [Required]
    [Column("Id_Category")]
    public int IdCategory { get; set; }

    [Required]
    [Column("Id_Mesure")]
    public int IdMesure { get; set; }

    [Required, MaxLength(50)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("Description_I")]
    public string? DescriptionI { get; set; }

    [MaxLength(50)]
    [Column("Description_II")]
    public string? DescriptionII { get; set; }

    [MaxLength(50)]
    [Column("Description_III")]
    public string? DescriptionIII { get; set; }

    [Required]
    public double Price { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? Qte { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public double? TotalPrice { get; set; }

    [Required]
    [Column("Id_Monais")]
    public int IdMonais { get; set; }

    [Required]
    [Column("Soeuil")]
    public int Soeuil { get; set; }

    public bool? Internal { get; set; }

    [Column("Can_Insert_After_0")]
    public bool? CanInsertAfter0 { get; set; }

    [Required, MaxLength(50)]
    [Column("User")]
    public string User { get; set; } = string.Empty;

    [Required]
    public DateOnly DateSys { get; set; }

    public DateOnly? DateEditing { get; set; }

    [Required, MaxLength(50)]
    [Column("Cumputer")]
    public string Cumputer { get; set; } = string.Empty;

    [Column("isTransferable")]
    public bool? IsTransferable { get; set; }

    /// <summary>Image optionnelle (URL ou data URI base64) pour affichage catalogue commandes.</summary>
    [Column("Image_Article")]
    public string? ImageArticle { get; set; }
}
