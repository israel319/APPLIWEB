using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Administration;

[Table("T_Users")]
public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Key]
    [MaxLength(50)]
    [Column("login")]
    public string Login { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Password { get; set; }

    [MaxLength(50)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string? Email { get; set; }

    public bool? Activated { get; set; }

    /// <summary>Devise préférée pour affichage et saisie (FK T_Moneys).</summary>
    [Column("Id_Monais_Pref")]
    public int? PreferredMoneyId { get; set; }

    // FK -> Role (un utilisateur a un seul role)
    public int? RoleId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }

    public ICollection<UserActivity> UserActivities { get; set; } = new List<UserActivity>();

    // Navigation: User -> UserLocalisations (1:N) - relation many-to-many via T_User_Localisations
    public ICollection<UserLocalisation> UserLocalisations { get; set; } = new List<UserLocalisation>();
}
