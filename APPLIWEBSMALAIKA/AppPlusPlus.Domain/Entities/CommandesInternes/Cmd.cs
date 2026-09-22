using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Domain.Entities.Fournisseurs;

namespace AppPlusPlus.Domain.Entities.CommandesInternes;

[Table("T_Cmds")]
public class Cmd
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Telephone { get; set; }

    [Column("Date_Required")]
    public DateOnly? DateRequired { get; set; }

    [Column("Date_Commande")]
    public DateOnly? DateCommande { get; set; }

    public int? Status { get; set; }

    public int? Type { get; set; }

    public DateTime? DateSys { get; set; }

    [MaxLength(50)]
    [Column("User")]
    public string? User { get; set; }

    [MaxLength(50)]
    [Column("Cumputer")]
    public string? Cumputer { get; set; }

    /// <summary>FK vers T_Supplier.</summary>
    public int? SupplierId { get; set; }

    /// <summary>Localisation cible pour les commandes génériques (réappro).</summary>
    [Column("Id_Localisation")]
    public int? LocalisationId { get; set; }

    // Navigation
    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }

    [ForeignKey(nameof(LocalisationId))]
    public Localisation? Localisation { get; set; }

    public ICollection<CmdDetail> Details { get; set; } = new List<CmdDetail>();
}
