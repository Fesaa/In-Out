using API.Constants;
using API.Data;
using API.Data.Repositories;
using API.DTOs;
using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ExportController(ILogger<ExportController> logger, IExportService exportService, IUnitOfWork unitOfWork): BaseApiController
{

    [HttpPost]
    [Authorize(PolicyConstants.HandleDeliveries)]
    public async Task<ActionResult<string>> Export(ExportRequestDto dto)
    {
        logger.LogInformation("Creating export on behalf of {UserId}", User.GetUserId());
        
        var deliveries =
            await unitOfWork.DeliveryRepository.GetDeliveryByIds(dto.DeliveryIds, DeliveryIncludes.Complete);
        if (deliveries.Count == 0)
        {
            return BadRequest();
        }


        var uuid = await exportService.Export(deliveries, dto);
        return Ok(uuid);
    }

    [HttpGet("{uuid}")]
    [Authorize(PolicyConstants.HandleDeliveries)]
    public async Task<ActionResult> GetExport(string uuid)
    {
        var export = await exportService.GetExport(uuid);
        if (export == null) return NotFound();
        
        return export;
    }
    
}