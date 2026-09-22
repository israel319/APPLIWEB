using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Application.Interfaces;

public interface IModuleLicenseService
{
    /// <summary>Retourne true si le module est actif (licencié).</summary>
    Task<bool> IsModuleActiveAsync(string moduleCode);

    /// <summary>Retourne tous les modules avec leur statut de licence.</summary>
    Task<IList<ModuleLicense>> GetAllModulesAsync();

    /// <summary>Active ou désactive un module.</summary>
    Task<bool> ToggleModuleAsync(int moduleId, bool isActive);

    /// <summary>Mise à jour complète d'une licence (prix, notes, expiration).</summary>
    Task<bool> UpdateLicenseAsync(ModuleLicense module);
}
