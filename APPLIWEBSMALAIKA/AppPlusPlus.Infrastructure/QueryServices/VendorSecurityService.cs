using System.Security.Cryptography;
using AppPlusPlus.Application.Interfaces;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.QueryServices;

/// <summary>
/// Implémentation sécurisée du service de mot de passe vendeur.
///
/// FORMAT DE STOCKAGE (clé "_sys.vlp" dans AppSettings) :
///   "{iterations}:{sel_base64}:{hash_base64}"
/// Exemple :
///   "100000:q8V7...==:ZxA2...=="
///
/// ALGORITHME : PBKDF2-HMAC-SHA512, 100 000 itérations, sel 32 bytes, sortie 64 bytes.
/// COMPARAISON : CryptographicOperations.FixedTimeEquals — résistant aux timing attacks.
/// </summary>
public class VendorSecurityService : IVendorSecurityService
{
    // Clé non-évidente dans AppSettings — le hash stocké est de toute façon illisible.
    private const string SettingKey = "_sys.vlp";
    private const int Iterations = 100_000;
    private const int SaltSize = 32;   // 256 bits
    private const int HashSize = 64;   // 512 bits

    private readonly IParametresRepository _repo;

    public VendorSecurityService(IParametresRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> IsPasswordConfiguredAsync()
    {
        var setting = await _repo.GetSettingAsync(SettingKey);
        return !string.IsNullOrWhiteSpace(setting?.Value);
    }

    public async Task<bool> VerifyPasswordAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;

        var setting = await _repo.GetSettingAsync(SettingKey);
        if (string.IsNullOrWhiteSpace(setting?.Value)) return false;

        return Verify(password, setting.Value);
    }

    public async Task SetPasswordAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Le mot de passe ne peut pas être vide.", nameof(password));

        var existing = await _repo.GetSettingAsync(SettingKey);
        if (existing?.Value != null)
            throw new InvalidOperationException("Un mot de passe vendeur est déjà configuré. Utilisez ChangePasswordAsync pour le modifier.");

        var hash = Hash(password);
        await _repo.SetSettingAsync(SettingKey, hash);
    }

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        if (!await VerifyPasswordAsync(currentPassword)) return false;

        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("Le nouveau mot de passe ne peut pas être vide.", nameof(newPassword));

        var hash = Hash(newPassword);
        await _repo.SetSettingAsync(SettingKey, hash);
        return true;
    }

    // ── Helpers cryptographiques ────────────────────────────────────────

    private static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            HashSize);

        return $"{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool Verify(string password, string storedValue)
    {
        var parts = storedValue.Split(':');
        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], out var iterations)) return false;

        byte[] salt, storedHash;
        try
        {
            salt       = Convert.FromBase64String(parts[1]);
            storedHash = Convert.FromBase64String(parts[2]);
        }
        catch { return false; }

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA512,
            storedHash.Length);

        // Comparaison à temps constant — empêche les timing attacks
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}
