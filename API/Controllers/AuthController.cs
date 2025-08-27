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
    
    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string returnUrl = "/")
    {
        var properties = new AuthenticationProperties { RedirectUri = returnUrl };
        return Challenge(properties, IdentityServiceExtensions.OpenIdConnect);
    }

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