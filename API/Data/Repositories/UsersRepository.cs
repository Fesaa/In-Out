using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public interface IUsersRepository
{
    Task<IList<UserDto>> GetAll();
    Task<IList<UserDto>> Search(string query);
    
    Task<bool> UserExists(string userId);
    Task<User?> GetByUserIdAsync(string userId);
    Task<User?> GetByUserIdAsync(int userId);
    Task<string?> GetLocaleAsync(string userId);
    Task<string?> GetLocaleAsync(int userId);
    
    void Add(User user);
    void Update(User user);
}

public class UsersRepository(DataContext context, IMapper mapper): IUsersRepository
{

    public async Task<IList<UserDto>> GetAll()
    {
        return await context.Users
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IList<UserDto>> Search(string query)
    {
        var normalizedQuery = query.ToNormalized();
        return await context.Users
            .Where(u => EF.Functions.Like(u.NormalizedName, $"%{normalizedQuery}%"))  
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<bool> UserExists(string userId)
    {
        return await context.Users
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId);
    }


    public async Task<User?> GetByUserIdAsync(string userId)
    {
        return await context.Users
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUserIdAsync(int userId)
    {
        return await context.Users
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetLocaleAsync(string userId)
    {
        return await context.Users
            .Where(x => x.UserId == userId)
            .Select(x => x.Language)
            .FirstOrDefaultAsync();
    }
    
    public async Task<string?> GetLocaleAsync(int userId)
    {
        return await context.Users
            .Where(x => x.Id == userId)
            .Select(x => x.Language)
            .FirstOrDefaultAsync();
    }

    public void Add(User user)
    {
        context.Users.Add(user).State = EntityState.Added;
    }

    public void Update(User user)
    {
        context.Users.Update(user).State = EntityState.Modified;
    }
}