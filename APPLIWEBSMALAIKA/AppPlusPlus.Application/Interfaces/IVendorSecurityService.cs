namespace AppPlusPlus.Application.Interfaces;

/// <summary>
/// Gestion du mot de passe vendeur qui protège l'interface de licences modules.
/// Le mot de passe n'est jamais stocké en clair : seul un hash PBKDF2-SHA512
/// avec sel aléatoire 32 bytes et 100 000 itérations est conservé en base.
/// </summary>
public interface IVendorSecurityService
{
    /// <summary>Indique si un mot de passe vendeur a déjà été défini.</summary>
    Task<bool> IsPasswordConfiguredAsync();

    /// <summary>
    /// Vérifie le mot de passe en comparaison à temps constant (protection contre timing attacks).
    /// Retourne true si correct, false sinon.
    /// </summary>
    Task<bool> VerifyPasswordAsync(string password);

    /// <summary>
    /// Définit le mot de passe vendeur (première configuration).
    /// Lève <see cref="InvalidOperationException"/> si un mot de passe existe déjà.
    /// </summary>
    Task SetPasswordAsync(string password);

    /// <summary>
    /// Change le mot de passe vendeur après vérification de l'ancien.
    /// Retourne true si réussi, false si l'ancien mot de passe est incorrect.
    /// </summary>
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
}
