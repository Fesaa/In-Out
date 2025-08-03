using API.Data;
using API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class StockController(ILogger<StockController> logger, IUnitOfWork unitOfWork): BaseApiController
{

    [HttpGet("/history/{stockId}")]
    public async Task<ActionResult<IList<StockHistoryDto>>> GetHistory(int stockId)
    {
        return Ok(await unitOfWork.StockRepository.GetHistoryDto(stockId));
    }

    [HttpGet]
    public async Task<ActionResult<IList<StockDto>>> GetStock()
    {
        return Ok(await unitOfWork.StockRepository.GetAllAsync());
    }
    
}