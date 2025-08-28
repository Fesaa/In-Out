using System.Security.Claims;
using API.Constants;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{

    public static string GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException();
    }

    public static string GetName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("name")?.Value ?? throw new UnauthorizedAccessException();
    }

    public static IList<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role)
            .Where(x => PolicyConstants.Roles.Contains(x.Value))
            .Select(x => x.Value)
            .ToList();
    }
    
}