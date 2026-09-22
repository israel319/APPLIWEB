using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Domain.Entities.Administration;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Infrastructure.Persistence.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(IDbContextFactory<AppDbContext> dbFactory) : base(dbFactory) { }

    // Override to prevent EF from trying to UPDATE the IDENTITY column 'Id' (non-PK)
    public override async Task UpdateAsync(User entity)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ctx.Set<User>().Update(entity);
        ctx.Entry(entity).Property(u => u.Id).IsModified = false;
        await ctx.SaveChangesAsync();
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Users.FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<User?> GetWithLocalisationsAsync(string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Users
            .Include(u => u.UserLocalisations).ThenInclude(ul => ul.Localisation)
            .FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<User?> GetWithActivitiesAsync(string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Users
            .Include(u => u.UserActivities).ThenInclude(ua => ua.Activity).ThenInclude(a => a!.Fonction)
            .FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<List<User>> GetByRoleAsync(int roleId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Users.Where(u => u.RoleId == roleId).ToListAsync();
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Users.Where(u => u.Activated == true).ToListAsync();
    }

    public async Task<List<UserLocalisation>> GetUserLocalisationsAsync(string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.UserLocalisations
            .Where(ul => ul.UserId == login)
            .Include(ul => ul.Localisation)
            .ToListAsync();
    }

    public async Task<List<UserActivity>> GetUserActivitiesAsync(string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.UserActivities
            .Where(ua => ua.UserLogin == login)
            .Include(ua => ua.Activity).ThenInclude(a => a!.Fonction)
            .ToListAsync();
    }

    public async Task SetUserLocalisationsAsync(string login, List<int> localisationIds)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var existing = await ctx.UserLocalisations.Where(ul => ul.UserId == login).ToListAsync();
        ctx.UserLocalisations.RemoveRange(existing);
        foreach (var locId in localisationIds)
        {
            ctx.UserLocalisations.Add(new UserLocalisation { UserId = login, LocalisationId = locId });
        }
        await ctx.SaveChangesAsync();
    }

    public async Task<User?> GetWithRoleAsync(string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<List<User>> GetByLocalisationAsync(int localisationId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.UserLocalisations
            .Where(ul => ul.LocalisationId == localisationId && ul.Activate == true)
            .Include(ul => ul.User)
            .Where(ul => ul.User != null)
            .Select(ul => ul.User!)
            .ToListAsync();
    }

    public async Task ReplaceUserActivitiesAsync(string login, List<int> activityIds)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var existing = await ctx.UserActivities.Where(ua => ua.UserLogin == login).ToListAsync();
        ctx.UserActivities.RemoveRange(existing);
        foreach (var actId in activityIds)
        {
            ctx.UserActivities.Add(new UserActivity
            {
                UserLogin = login,
                ActivityId = actId,
                IsGranted = true,
                AssignedDate = DateTime.Now
            });
        }
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateUserLocalisationsAsync(string login, HashSet<int> newLocalisationIds)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var existing = await ctx.UserLocalisations
            .Where(ul => ul.UserId == login)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);

        // Deactivate localisations that are no longer in the new set
        foreach (var ul in existing.Where(ul => ul.Activate == true))
        {
            if (ul.LocalisationId.HasValue && !newLocalisationIds.Contains(ul.LocalisationId.Value))
            {
                ul.Activate = false;
                ul.EndDate = today;
            }
        }

        // Determine which localisation IDs already have an active record
        var activeLocIds = existing
            .Where(ul => ul.Activate == true && ul.LocalisationId.HasValue)
            .Select(ul => ul.LocalisationId!.Value)
            .ToHashSet();

        // Add new localisations that don't already exist as active
        foreach (var locId in newLocalisationIds)
        {
            if (!activeLocIds.Contains(locId))
            {
                ctx.UserLocalisations.Add(new UserLocalisation
                {
                    UserId = login,
                    LocalisationId = locId,
                    BeginDate = today,
                    Activate = true
                });
            }
        }

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        // Remove dependent records first
        var activities = await ctx.UserActivities.Where(a => a.UserLogin == login).ToListAsync();
        ctx.UserActivities.RemoveRange(activities);

        var localisations = await ctx.UserLocalisations.Where(l => l.UserId == login).ToListAsync();
        ctx.UserLocalisations.RemoveRange(localisations);

        var caisses = await ctx.UserCaisses.Where(c => c.User == login).ToListAsync();
        ctx.UserCaisses.RemoveRange(caisses);

        var fonctions = await ctx.UserFonctions.Where(f => f.User == login).ToListAsync();
        ctx.UserFonctions.RemoveRange(fonctions);

        var user = await ctx.Users.FindAsync(login);
        if (user != null)
            ctx.Users.Remove(user);

        await ctx.SaveChangesAsync();
    }

    public async Task<int?> GetPreferredMoneyIdAsync(string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Users
            .AsNoTracking()
            .Where(u => u.Login.ToLower() == login.ToLower())
            .Select(u => u.PreferredMoneyId)
            .FirstOrDefaultAsync();
    }

    public async Task SetPreferredMoneyIdAsync(string login, int moneyId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        await ctx.Users
            .Where(u => u.Login.ToLower() == login.ToLower())
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.PreferredMoneyId, moneyId));
    }
}
