using API.Constants;
using API.Data;
using API.Data.Repositories;
using API.DTOs;
using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Policy = PolicyConstants.ManageStock)]
    public async Task<IActionResult> UpdateStock(StockDto stock)
    {
        await stockService.UpdateStockAsync(stock);
        return Ok();
    }

    [HttpPost]
    [Authorize(Policy = PolicyConstants.ManageStock)]
    public async Task<IActionResult> UpdateStock(UpdateStockDto dto)
    {
        var user = await unitOfWork.UsersRepository.GetByUserIdAsync(User.GetUserId());
        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        if (string.IsNullOrWhiteSpace(dto.Reference))
        {
            dto.Reference = $"Manual stock update on {DateTime.UtcNow.ToShortDateString()} @ {DateTime.UtcNow.ToLongTimeString()} by {user.Name}";
        }
        
        var res = await stockService.UpdateStockBulkAsync(user, [dto]);
        if (res.IsFailure)
        {
            return BadRequest(res.Error);
        }
        return Ok();
    }
    
}