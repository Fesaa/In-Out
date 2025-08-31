using API.Constants;
using API.Data;
using API.Data.Repositories;
using API.DTOs;
using API.Extensions;
using API.Services.Exporters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ExportController(ILogger<ExportController> logger, IExporter exporter, IUnitOfWork unitOfWork): BaseApiController
{

    [HttpPost]
    [Authorize(PolicyConstants.HandleDeliveries)]
    public async Task<ActionResult<FileResult>> Export(ExportRequestDto dto)
    {
        logger.LogInformation("Creating export on behalf of {UserId}", User.GetUserId());
        
        var deliveries =
            await unitOfWork.DeliveryRepository.GetDeliveryByIds(dto.DeliveryIds, DeliveryIncludes.Complete);
        if (deliveries.Count == 0)
        {
            return NotFound();
        }


        var result = await exporter.Export(deliveries, dto);
        return Ok(result);
    }
    
}