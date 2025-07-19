using System.Security.Claims;

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
    
}