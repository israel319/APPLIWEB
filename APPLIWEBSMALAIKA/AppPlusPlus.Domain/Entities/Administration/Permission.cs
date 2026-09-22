using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Administration;

/// <summary>
/// T_Permissions -- Droits d'acces d'un role sur une fonction.
/// Combinaison unique (RoleId, FonctionId).
/// </summary>
[Table("T_Permissions")]
public class Permission
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PermissionId { get; set; }

    [Required]
    public int RoleId { get; set; }

    [Required]
    public int FonctionId { get; set; }

    /// <summary>Droit de lecture / consultation.</summary>
    public bool CanRead { get; set; } = true;

    /// <summary>Droit d'ecriture / modification / creation.</summary>
    public bool CanWrite { get; set; }

    /// <summary>Droit de suppression.</summary>
    public bool CanDelete { get; set; }

    // Navigation
    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }

    [ForeignKey(nameof(FonctionId))]
    public Fonction? Fonction { get; set; }
}
