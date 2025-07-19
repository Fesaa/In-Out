using System.Security.Claims;
using API.Data;
using API.Entities;
using API.Extensions;

namespace API.Services;

public interface IUserService
{
    /// <summary>
    /// Gets the user, creates if not found
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    Task<User> GetUser(ClaimsPrincipal principal);
}

public class UserService(IUnitOfWork unitOfWork): IUserService
{

    public async Task<User> GetUser(ClaimsPrincipal principal)
    {
        var user = await unitOfWork.UsersRepository.GetByUserIdAsync(principal.GetUserId());
        if (user == null) return await NewUser(principal);

        var name = principal.GetName();
        var normalizedName = name.ToNormalized();

        if (user.NormalizedName != normalizedName)
        {
            user.Name = name;
            user.NormalizedName = normalizedName;
            unitOfWork.UsersRepository.Update(user);
            await unitOfWork.CommitAsync();
        }
            
        return user;
    }
    
    private async Task<User> NewUser(ClaimsPrincipal principal)
    {
        var user = DefaultUser(principal.GetUserId());
        user.Name = principal.GetName();
        user.NormalizedName = user.Name.ToNormalized();
        
        unitOfWork.UsersRepository.Add(user);
        await unitOfWork.CommitAsync();
        
        return user;
    }
    
    private User DefaultUser(string userId)
    {
        return new User()
        {
            UserId = userId,
            Language = "en",
        };
    }
}