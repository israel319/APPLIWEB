using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByLoginAsync(string login);
    Task<User?> GetWithLocalisationsAsync(string login);
    Task<User?> GetWithActivitiesAsync(string login);
    Task<List<User>> GetByRoleAsync(int roleId);
    Task<List<User>> GetActiveUsersAsync();
    Task<List<UserLocalisation>> GetUserLocalisationsAsync(string login);
    Task<List<UserActivity>> GetUserActivitiesAsync(string login);
    Task SetUserLocalisationsAsync(string login, List<int> localisationIds);

    /// <summary>
    /// Returns the user with Role navigation loaded.
    /// </summary>
    Task<User?> GetWithRoleAsync(string login);

    /// <summary>
    /// Returns all users linked to the given localisation (active UserLocalisation records).
    /// </summary>
    Task<List<User>> GetByLocalisationAsync(int localisationId);

    /// <summary>
    /// Replaces all UserActivity records for the given user login.
    /// Deletes existing records and inserts new ones for the provided activity IDs.
    /// </summary>
    Task ReplaceUserActivitiesAsync(string login, List<int> activityIds);

    /// <summary>
    /// Smart update of UserLocalisations: deactivates removed localisations
    /// (sets Activate=false, EndDate=today) and activates new ones (Activate=true, BeginDate=today).
    /// </summary>
    Task UpdateUserLocalisationsAsync(string login, HashSet<int> newLocalisationIds);
    Task DeleteUserAsync(string login);

    Task<int?> GetPreferredMoneyIdAsync(string login);
    Task SetPreferredMoneyIdAsync(string login, int moneyId);
}
