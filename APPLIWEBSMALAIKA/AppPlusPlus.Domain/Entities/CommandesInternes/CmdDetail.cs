using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Catalogue;

namespace AppPlusPlus.Domain.Entities.CommandesInternes;

[Table("T_Cmd_Details")]
public class CmdDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Id_Cmd")]
    public int? IdCmd { get; set; }

    [MaxLength(100)]
    [Column("Id_Article")]
    public string? IdArticle { get; set; }

    [Column("Qte_Order")]
    public double? QteOrder { get; set; }

    [Column("Qte_Receive")]
    public double? QteReceive { get; set; }

    public DateOnly? DateSys { get; set; }

    [MaxLength(50)]
    [Column("User")]
    public string? User { get; set; }

    [MaxLength(50)]
    [Column("Cumputer")]
    public string? Cumputer { get; set; }

    // Navigation
    [ForeignKey(nameof(IdCmd))]
    public Cmd? Cmd { get; set; }

    [ForeignKey(nameof(IdArticle))]
    public Article? Article { get; set; }
}
