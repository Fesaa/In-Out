using API.Constants;
using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(PolicyConstants.ManageApplication)]
public class SettingsController(ILogger<SettingsController> logger, ISettingsService settingsService): BaseApiController
{

    [HttpGet]
    public async Task<ActionResult<ServerSettingsDto>> GetSettings()
    {
        var settings = await settingsService.GetSettingsAsync();
        return Ok(settings);
    }

    [HttpPost]
    public async Task<AcceptedResult> SaveSettings(ServerSettingsDto settings)
    {
        await settingsService.SaveSettingsAsync(settings);
        return Accepted();
    }
    
}