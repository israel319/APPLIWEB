using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Application.Services.Localisation;

public interface ILocalisationService
{
    /// <summary>
    /// Returns the localisation IDs assigned to the user.
    /// Admins get an empty list (meaning no filter / access to all).
    /// </summary>
    Task<List<int>> GetUserLocalisationIdsAsync(string login, bool isAdmin);

    /// <summary>
    /// Returns localisations filtered by the given IDs.
    /// Admins or empty list returns all localisations.
    /// </summary>
    Task<List<Domain.Entities.Administration.Localisation>> GetFilteredLocalisationsAsync(
        List<int> localisationIds, bool isAdmin);

    /// <summary>
    /// Returns every localisation, ordered by description.
    /// </summary>
    Task<List<Domain.Entities.Administration.Localisation>> GetAllLocalisationsAsync();

    /// <summary>
    /// Returns a single localisation by its ID, or null if not found.
    /// </summary>
    Task<Domain.Entities.Administration.Localisation?> GetLocalisationByIdAsync(int id);
}
