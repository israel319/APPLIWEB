using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Finance;

[Table("T_Periodes")]
public class Periode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    [MaxLength(50)]
    public string? Comment { get; set; }

    public bool? Activated { get; set; }

    [MaxLength(50)]
    public string? UserLogin { get; set; }

    [Required]
    public DateOnly DateSys { get; set; }
}
