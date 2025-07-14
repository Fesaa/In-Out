using API.DTOs;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ConfigurationController(IConfiguration configuration): BaseApiController
{

    [AllowAnonymous]
    [HttpGet("oidc")]
    public ActionResult<OidcConfigurationDto> GetOidcConfiguration()
    {
        var openIdConnectConfig = configuration.GetSection(IdentityServiceExtensions.OpenIdConnect).Get<OidcConfigurationDto>();
        if (openIdConnectConfig == null)
            throw new Exception("OpenIdConnect configuration is missing");
        
        return Ok(openIdConnectConfig);
    }
    
}