using API.Constants;
using API.Data;
using API.Data.Repositories;
using API.DTOs;
using API.DTOs.Filter;
using API.Entities.Enums;
using API.Extensions;
using API.Helpers;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class DeliveryController(ILogger<DeliveryController> logger,
    IUnitOfWork unitOfWork, IDeliveryService deliveryService,
    IUserService userService, IMapper mapper): BaseApiController
{

    /// <summary>
    /// Get delivery by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<DeliveryDto?>> Get(int id)
    {
        var delivery = await unitOfWork.DeliveryRepository.GetDelivery(id, DeliveryIncludes.Complete);
        if (delivery == null) return NotFound();
        return Ok(delivery);
    }

    /// <summary>
    /// Filter deliveries
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    [HttpPost("filter")]
    public async Task<IList<DeliveryDto>> GetDeliveries([FromBody] FilterDto filter, [FromQuery] PaginationParams pagination)
    {
        var user = await unitOfWork.UsersRepository.GetByUserIdAsync(User.GetUserId());
        if (user == null)
        {
            logger.LogError("User {UserId} not found", User.GetUserId());
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

    /// <summary>
    /// Create a new delivery
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<DeliveryDto>> Create(DeliveryDto dto)
    {
        var user = await userService.GetUser(User);
        var delivery = await deliveryService.CreateDelivery(user.Id, dto);

        return Ok(mapper.Map<DeliveryDto>(delivery));
    }

    /// <summary>
    /// Update an existing delivery
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> Update(DeliveryDto dto)
    {
        var delivery = await deliveryService.UpdateDelivery(User, dto);
        return Ok(mapper.Map<DeliveryDto>(delivery));
    }

    /// <summary>
    /// Change the state of a delivery
    /// </summary>
    /// <param name="deliveryId"></param>
    /// <param name="nextState"></param>
    /// <returns></returns>
    [HttpPost("transition")]
    public async Task<IActionResult> UpdateState([FromQuery] int deliveryId, [FromQuery] DeliveryState nextState)
    {
        await deliveryService.TransitionDelivery(User, deliveryId, nextState);
        return Ok();
    }

    /// <summary>
    /// Fully delete a delivery, this is destructive. Consider transition to <see cref="DeliveryState.Cancelled"/>
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(PolicyConstants.HandleDeliveries)]
    public async Task<IActionResult> Delete(int id)
    {
        await deliveryService.DeleteDelivery(id);
        return Ok();
    }
    
}