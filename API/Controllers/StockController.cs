using API.Data;
using API.Data.Repositories;
using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class StockController(ILogger<StockController> logger, IUnitOfWork unitOfWork, IStockService stockService): BaseApiController
{

    [HttpGet("history/{stockId}")]
    public async Task<ActionResult<IList<StockHistoryDto>>> GetHistory(int stockId)
    {
        return Ok(await unitOfWork.StockRepository.GetHistoryDto(stockId));
    }

    [HttpGet]
    public async Task<ActionResult<IList<StockDto>>> GetStock()
    {
        return Ok(await unitOfWork.StockRepository.GetAllDtoAsync(StockIncludes.Product));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateStock(StockDto stock)
    {
        await stockService.UpdateStockAsync(stock);
        return Ok();
    }
    
}