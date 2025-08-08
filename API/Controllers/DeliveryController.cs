using API.Constants;
using API.Data;
using API.Data.Repositories;
using API.DTOs;
using API.DTOs.Filter;
using API.Extensions;
using API.Helpers;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class DeliveryController(ILogger<DeliveryController> logger, IUnitOfWork unitOfWork, IDeliveryService deliveryService, IUserService userService): BaseApiController
{

    [HttpGet("{id}")]
    public async Task<ActionResult<DeliveryDto?>> Get(int id)
    {
        var delivery = await unitOfWork.DeliveryRepository.GetDelivery(id, DeliveryIncludes.Complete);
        if (delivery == null) return NotFound();
        return Ok(delivery);
    }

    [HttpPost("filter")]
    public async Task<IList<DeliveryDto>> GetDeliveries([FromBody] FilterDto filter, [FromQuery] PaginationParams pagination)
    {
        var user = await unitOfWork.UsersRepository.GetByUserIdAsync(User.GetUserId());
        if (user == null)
        {
            logger.LogError($"User {User.GetUserId()} not found");
            throw new UnauthorizedAccessException();
        }
        
        if (User.IsInRole(PolicyConstants.ViewAllDeliveries))
        {
            return await unitOfWork.DeliveryRepository.GetDeliveries(filter, pagination);
        }
        
        // Ensure user only sees their own deliveries
        filter.Statements = filter.Statements
            .Where(s => s.Field != FilterField.From)
            .ToList();
            
        filter.Statements.Add(new FilterStatementDto
        {
            Field = FilterField.From,
            Comparison = FilterComparison.Equals,
            Value = user.Id.ToString(),
        });
        
        
        return await unitOfWork.DeliveryRepository.GetDeliveries(filter, pagination);
    }

    [HttpPost]
    public async Task<IActionResult> Create(DeliveryDto dto)
    {
        var user = await userService.GetUser(User);

        try
        {
            await deliveryService.CreateDelivery(user.Id, dto);
        }
        catch (ApplicationException e)
        {
            return BadRequest(e.Message);
        }
        
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update(DeliveryDto dto)
    {
        try
        {
            await deliveryService.UpdateDelivery(User, dto);
        }
        catch (ApplicationException e)
        {
            return BadRequest(e.Message);
        }
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await deliveryService.DeleteDelivery(id);
        } 
        catch (ApplicationException e)
        {
            return BadRequest(e.Message);
        }
        return Ok();
    }
    
}