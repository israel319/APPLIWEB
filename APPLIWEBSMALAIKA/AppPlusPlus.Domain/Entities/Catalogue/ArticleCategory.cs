using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Catalogue;

[Table("T_Art_Categorys")]
public class ArticleCategory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id_Category")]
    public int IdCategory { get; set; }

    [MaxLength(50)]
    [Column("Description_Category")]
    public string? DescriptionCategory { get; set; }

    /// <summary>Afficher cette catégorie sur l'accueil catalogue client.</summary>
    [Column("Visible_Catalog")]
    public bool VisibleCatalog { get; set; } = true;

    /// <summary>Image représentative (URL ou data URI base64).</summary>
    [Column("Image_Category")]
    public string? ImageCategory { get; set; }
}
