using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Finance;

[Table("T_Audit")]
public class Audit
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    public string? Fonction { get; set; }

    [MaxLength(50)]
    public string? Operation { get; set; }

    [MaxLength(50)]
    public string? RefId { get; set; }

    [MaxLength(50)]
    public string? Comment { get; set; }

    public DateTime? Date { get; set; }

    [MaxLength(50)]
    public string? UserLogin { get; set; }
}
