namespace AppPlusPlus.Domain.Entities.Administration;

/// <summary>
/// Représente la licence d'activation d'un module de l'application.
/// Permet de vendre l'application module par module.
/// </summary>
public class ModuleLicense
{
    public int Id { get; set; }

    /// <summary>Code unique du module — correspond aux constantes AppFunctions.</summary>
    public string ModuleCode { get; set; } = null!;

    /// <summary>Nom affiché dans l'interface.</summary>
    public string ModuleName { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>Icône Bootstrap Icons (ex: "bi-receipt").</summary>
    public string? Icon { get; set; }

    /// <summary>Module actif = accessible aux utilisateurs autorisés.</summary>
    public bool IsActive { get; set; }

    /// <summary>Module noyau : toujours actif, non désactivable (Dashboard, Administration, Paramètres).</summary>
    public bool IsCore { get; set; }

    /// <summary>Prix de référence du module (pour affichage commercial).</summary>
    public decimal? Price { get; set; }

    /// <summary>Date à laquelle le module a été activé.</summary>
    public DateTime? ActivatedAt { get; set; }

    /// <summary>Date d'expiration de la licence (null = illimitée).</summary>
    public DateTime? ExpiresAt { get; set; }

    public string? LicenseNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
