using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Finance;

[Table("T_Taux")]
public class Taux
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("Taux", TypeName = "decimal(18,0)")]
    public decimal TauxValue { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? RateLow { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    public decimal? RateUp { get; set; }

    public DateOnly? Date { get; set; }

    [MaxLength(50)]
    public string? Comment { get; set; }

    [MaxLength(50)]
    public string? UserLogin { get; set; }

    public DateOnly? DateSys { get; set; }
}
