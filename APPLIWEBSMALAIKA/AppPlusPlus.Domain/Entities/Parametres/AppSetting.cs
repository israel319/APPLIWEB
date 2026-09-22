using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Parametres;

/// <summary>
/// Correspond a la table [dbo].[T_AppSettings] -- Parametres globaux de l'application (cle/valeur).
/// </summary>
[Table("T_AppSettings")]
public class AppSetting
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Value { get; set; }

    public ICollection<ShopProfile> ShopProfiles { get; set; } = new List<ShopProfile>();
}
