using System.Security.Claims;
using API.Constants;
using API.Data;
using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Entities.Enums;
using API.Extensions;

namespace API.Services;

public interface IDeliveryService
{
    Task<Delivery> CreateDelivery(int userId, DeliveryDto  dto);
    Task<Delivery> UpdateDelivery(ClaimsPrincipal actor, DeliveryDto dto);
    Task DeleteDelivery(int id);
    Task TransitionDelivery(ClaimsPrincipal actor, int deliveryId, DeliveryState nextState);
}

public class DeliveryService(ILogger<DeliveryService> logger, IUnitOfWork unitOfWork, IUserService userService, IStockService stockService): IDeliveryService
{
    private static readonly IList<DeliveryState> FinalDeliveryStates = [DeliveryState.Completed, DeliveryState.Cancelled, DeliveryState.Handled];

    public async Task<Delivery> CreateDelivery(int userId, DeliveryDto dto)
    {
        var user = await unitOfWork.UsersRepository.GetByUserIdAsync(userId);
        if (user == null)
            throw new ApplicationException("errors.client-not-found");
        
        var client = await unitOfWork.ClientRepository.GetClientById(dto.ClientId);
        if (client == null)
            throw new ApplicationException("errors.client-not-found");

        var lines = dto.Lines.GroupBy(l => l.ProductId)
            .Select(g => new DeliveryLineDto
            {
                ProductId = g.Key,
                Quantity = g.Sum(l => l.Quantity),
            })
            .Select(deliveryLineDto => new DeliveryLine
            {
                ProductId = deliveryLineDto.ProductId,
                Quantity = deliveryLineDto.Quantity,
            }).ToList();

        var productLookup = (await unitOfWork.ProductRepository
            .GetByIds(lines.Select(l => l.ProductId)))
            .ToDictionary(p => p.Id, p => p);

        var delivery = new Delivery
        {
            State = DeliveryState.InProgress,
            UserId = userId,
            Recipient = client,
            Message = dto.Message.Trim(),
            Lines = lines,
            SystemMessages = [],
        };
        
        var result = await stockService.UpdateStockBulkAsync(user, lines
            .Where(l => productLookup[l.ProductId].IsTracked)
            .Select(l => new UpdateStockDto 
            { 
                ProductId = l.ProductId, 
                Value = l.Quantity, 
                Operation = StockOperation.Remove, 
                Reference = $"Delivery creation by {user.Name} to {client.Name}",
            })
            .ToList());

        if (result.IsFailure)
        {
            throw new ApplicationException(result.Error);
        }
        
        unitOfWork.DeliveryRepository.Add(delivery);
        await unitOfWork.CommitAsync();

        return delivery;
    }

    public async Task<Delivery> UpdateDelivery(ClaimsPrincipal actor, DeliveryDto dto)
    {
        var delivery = await unitOfWork.DeliveryRepository.GetDeliveryById(dto.Id, DeliveryIncludes.Complete);
        if (delivery == null)
            throw new ApplicationException("errors.delivery-not-found");
        
        if (FinalDeliveryStates.Contains(delivery.State))
            throw new ApplicationException("errors.delivery-locked");
        
        if (delivery.Recipient.Id != dto.ClientId)
            throw new ApplicationException("errors.cannot-change-recipient");
        
        var user = await userService.GetUser(actor);
        if (delivery.UserId != user.Id && !actor.IsInRole(PolicyConstants.CreateForOthers))
            throw new UnauthorizedAccessException();

        delivery.Message = dto.Message;

        if (delivery.UserId != dto.FromId)
        {
            if (user.Id != dto.FromId && !actor.IsInRole(PolicyConstants.CreateForOthers))
                throw new UnauthorizedAccessException();
            
            delivery.UserId = dto.FromId;
        }
        
        // Ensure no duplicates
        var dtoLines = dto.Lines.GroupBy(l => l.ProductId)
            .Select(g => new DeliveryLineDto
            {
                ProductId = g.Key,
                Quantity = g.Sum(l => l.Quantity),
            }).ToList();
        
        var linesLookup = delivery.Lines.ToDictionary(l => l.ProductId, l => l);
        var productLookup = (await unitOfWork.ProductRepository
                .GetByIds(dtoLines.Select(l => l.ProductId)))
            .ToDictionary(p => p.Id, p => p);

        var newLines = dtoLines.Select(deliveryLineDto => new DeliveryLine
        {
            ProductId = deliveryLineDto.ProductId,
            Quantity = deliveryLineDto.Quantity,
        }).ToList();
        
        var updates = dtoLines
            .Where(l => productLookup[l.ProductId].IsTracked)
            .Select(l =>
            {
                if (!linesLookup.TryGetValue(l.ProductId, out var deliveryLine))
                {
                    return new UpdateStockDto
                    {
                        ProductId = l.ProductId,
                        Value = l.Quantity,
                        Operation = StockOperation.Remove,
                        Reference = $"Delivery {delivery.Id} update to {delivery.Recipient.Name} by {user.Name}",
                    };
                }

                var diff = l.Quantity - deliveryLine.Quantity;
                if (diff == 0)
                {
                    return null;
                }

                logger.LogDebug("Updating stock for product {ProductId} with diff {diff}", deliveryLine.ProductId, diff);
                return new UpdateStockDto
                {
                    ProductId = deliveryLine.ProductId,
                    Value = Math.Abs(diff),
                    Operation = diff < 0 ? StockOperation.Add : StockOperation.Remove,
                    Reference = $"Delivery {delivery.Id} update to {delivery.Recipient.Name} by {user.Name}",
                };
            })
            .RequireNotNull()
            .ToList();

        if (updates.Count == 0)
        {
            unitOfWork.DeliveryRepository.Update(delivery);
            await unitOfWork.CommitAsync();
            return delivery;
        }

        logger.LogDebug("Delivery update resulted in {Length} stock updates", updates.Count);
        
        var result = await stockService.UpdateStockBulkAsync(user, updates);
        if (result.IsFailure)
        {
            throw new ApplicationException(result.Error);
        }
        
        unitOfWork.DeliveryRepository.RemoveRange(delivery.Lines);
        delivery.Lines = newLines;
        
        unitOfWork.DeliveryRepository.Update(delivery);
        await unitOfWork.CommitAsync();

        return delivery;
    }

    public async Task DeleteDelivery(int id)
    {
        var delivery = await unitOfWork.DeliveryRepository.GetDeliveryById(id);
        if (delivery == null)
            throw new ApplicationException("errors.delivery-not-found");
        
        unitOfWork.DeliveryRepository.Remove(delivery);
        await unitOfWork.CommitAsync();
    }

    public async Task TransitionDelivery(ClaimsPrincipal actor, int deliveryId, DeliveryState nextState)
    {
        var canHandleDeliveries = actor.IsInRole(PolicyConstants.HandleDeliveries);
        
        var delivery = await unitOfWork.DeliveryRepository.GetDeliveryById(deliveryId);
        if (delivery == null)
            throw new ApplicationException("errors.delivery-not-found");

        var validNextStates = GetStateOptions(delivery.State, canHandleDeliveries);
        if (!validNextStates.Contains(nextState))
            throw new ApplicationException("errors.invalid-next-state");

        delivery.State = nextState;
        await unitOfWork.CommitAsync();
    }
    
    private static List<DeliveryState> GetStateOptions(DeliveryState currentState, bool canHandleDeliveries)
    {
        switch (currentState)
        {
            case DeliveryState.InProgress:
                return [DeliveryState.Completed, DeliveryState.Cancelled,];

            case DeliveryState.Completed:
                var states = new List<DeliveryState>
                {
                    DeliveryState.InProgress,
                    DeliveryState.Cancelled,
                };

                if (canHandleDeliveries)
                {
                    states.Add(DeliveryState.Handled);
                }

                return states;

            case DeliveryState.Handled:
                return canHandleDeliveries ? [DeliveryState.Completed] : [];

            case DeliveryState.Cancelled:
            default:
                return [];

        }
    }

}