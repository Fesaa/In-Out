using API.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public interface IUsersRepository
{
    Task<bool> UserExists(string userId);
    Task<User> GetByUserIdAsync(string userId);
    Task<string> GetLocaleAsync(string userId);
    Task EnsureExistsAsync(string userId);
}

public class UsersRepository(DataContext context, IMapper mapper): IUsersRepository
{

    private User DefaultUserPreferences(string userId)
    {
        return new User()
        {
            UserId = userId,
            Language = "en",
        };
    }

    public async Task<bool> UserExists(string userId)
    {
        return await context.Users
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId);
    }


    public async Task<User> GetByUserIdAsync(string userId)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(user => user.UserId == userId);

        if (user == null)
        {
            user = DefaultUserPreferences(userId);
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        return user;
    }

    public async Task EnsureExistsAsync(string userId)
    {
        await GetByUserIdAsync(userId);
    }

    public async Task<string> GetLocaleAsync(string userId)
    {
        var user = await GetByUserIdAsync(userId);
        return user.Language;
    }
}