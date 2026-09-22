using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Catalogue;

[Table("T_Art_Types")]
public class ArticleType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id_Type")]
    public int IdType { get; set; }

    [MaxLength(100)]
    [Column("Description_Type")]
    public string? DescriptionType { get; set; }
}
