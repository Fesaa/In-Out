using API.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public interface IUserPreferencesRepository
{
    Task<bool> UserExists(string userId);
    Task<UserPreferences> GetByUserIdAsync(string userId);
    Task<string?> GetLocaleAsync(string userId);
    Task EnsureExistsAsync(string userId);
}

public class UserPreferencesRepository(DataContext context, IMapper mapper): IUserPreferencesRepository
{

    private UserPreferences DefaultUserPreferences(string userId)
    {
        return new UserPreferences()
        {
            UserId = userId,
            Language = "en",
        };
    }

    public async Task<bool> UserExists(string userId)
    {
        return await context.UserPreferences
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId);
    }


    public async Task<UserPreferences> GetByUserIdAsync(string userId)
    {
        var pref = await context.UserPreferences
            .FirstOrDefaultAsync(pref => pref.UserId == userId);

        if (pref == null)
        {
            pref = DefaultUserPreferences(userId);
            await context.UserPreferences.AddAsync(pref);
            await context.SaveChangesAsync();
        }

        return pref;
    }

    public async Task EnsureExistsAsync(string userId)
    {
        await GetByUserIdAsync(userId);
    }

    public async Task<string?> GetLocaleAsync(string userId)
    {
        return await context.UserPreferences
            .Where(pref => pref.UserId == userId)
            .Select(pref => pref.Language)
            .FirstOrDefaultAsync();
    }
}