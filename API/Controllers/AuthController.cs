using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
public class AuthController: ControllerBase
{
    /// <summary>
    /// Trigger OIDC login flow
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        var properties = new AuthenticationProperties { RedirectUri = returnUrl };
        return Challenge(properties, IdentityServiceExtensions.OpenIdConnect);
    }

    /// <summary>
    /// Trigger OIDC logout flow, if no auth cookie is found. Redirects to root
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("logout")]
    public IActionResult Logout()
    {
        if (!Request.Cookies.ContainsKey(OidcService.CookieName))
        {
            return Redirect("/");
        }
        
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/login" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            IdentityServiceExtensions.OpenIdConnect);
    }
    
}