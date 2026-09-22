using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Application.Services.Localisation;

public class LocalisationService : ILocalisationService
{
    private readonly IUserRepository _userRepo;
    private readonly ILookupRepository _lookupRepo;

    public LocalisationService(IUserRepository userRepo, ILookupRepository lookupRepo)
    {
        _userRepo = userRepo;
        _lookupRepo = lookupRepo;
    }

    /// <inheritdoc />
    public async Task<List<int>> GetUserLocalisationIdsAsync(string login, bool isAdmin)
    {
        if (isAdmin)
            return new List<int>();

        var userLocs = await _userRepo.GetUserLocalisationsAsync(login);

        return userLocs
            .Where(ul => ul.Activate == true && ul.LocalisationId.HasValue)
            .Select(ul => ul.LocalisationId!.Value)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<List<Domain.Entities.Administration.Localisation>> GetFilteredLocalisationsAsync(
        List<int> localisationIds, bool isAdmin)
    {
        var all = await _lookupRepo.GetAllLocalisationsAsync();

        if (isAdmin || !localisationIds.Any())
            return all.OrderBy(l => l.DescriptionLocalisation).ToList();

        return all
            .Where(l => localisationIds.Contains(l.IdLocalisation))
            .OrderBy(l => l.DescriptionLocalisation)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<List<Domain.Entities.Administration.Localisation>> GetAllLocalisationsAsync()
    {
        var all = await _lookupRepo.GetAllLocalisationsAsync();
        return all.OrderBy(l => l.DescriptionLocalisation).ToList();
    }

    /// <inheritdoc />
    public async Task<Domain.Entities.Administration.Localisation?> GetLocalisationByIdAsync(int id)
        => await _lookupRepo.GetLocalisationByIdAsync(id);
}
