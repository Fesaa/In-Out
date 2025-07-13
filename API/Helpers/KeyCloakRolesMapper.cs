using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace API.Helpers;

public class KeyCloakRolesMapper: IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity == null || !principal.Identity.IsAuthenticated)
        {
            return principal;
        }

        var identity = (ClaimsIdentity)principal.Identity;
        var resourceAccess = identity.FindFirst("resource_access");
        if (resourceAccess == null || string.IsNullOrWhiteSpace(resourceAccess.Value))
        {
            return principal;
        }

        var resources = JsonSerializer.Deserialize<Dictionary<string, Resource>>(resourceAccess.Value);
        if (resources == null || resources.Count == 0)
        {
            return principal;
        }

        // TODO: Get key from configuration
        var resource = resources.GetValueOrDefault("in-out");
        if (resource?.roles == null)
        {
            return principal;
        }

        foreach (var role in resource.roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return principal;
    }

    private class Resource
    {
        // ReSharper disable once InconsistentNaming
        public List<string>? roles { get; set; }
    }
}