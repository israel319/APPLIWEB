using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Approvisionnement;

[Table("T_Transformation")]
public class Transformation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TransformationId { get; set; }

    [Required, MaxLength(50)]
    public string FromArticleId { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string ToArticleId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Qte { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ToQte { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required, MaxLength(50)]
    public string Comment { get; set; } = string.Empty;

    [Required]
    public DateTime DateSys { get; set; }

    [Required, MaxLength(50)]
    public string UserLogin { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Computer { get; set; } = string.Empty;

    public int? FromLocalisationId { get; set; }

    public int? ToLocalisationId { get; set; }

    public int? Status { get; set; }
}
