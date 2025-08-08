using System.Security.Claims;
using API.Constants;
using API.Data;
using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Entities.Enums;

namespace API.Services;

public interface IDeliveryService
{
    Task CreateDelivery(int userId, DeliveryDto  dto);
    Task UpdateDelivery(ClaimsPrincipal actor, DeliveryDto dto);
    Task DeleteDelivery(int id);
}

public class DeliveryService(IUnitOfWork unitOfWork, IUserService userService, IStockService stockService): IDeliveryService
{
    public static readonly IList<DeliveryState> FinalDeliveryStates = [DeliveryState.Completed, DeliveryState.Canceled];

    public async Task CreateDelivery(int userId, DeliveryDto dto)
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
                // TODO: Reference and notes?
            })
            .ToList());

        if (result.IsFailure)
        {
            throw new ApplicationException(result.Error);
        }
        
        unitOfWork.DeliveryRepository.Add(delivery);
        await unitOfWork.CommitAsync();
    }

    public async Task UpdateDelivery(ClaimsPrincipal actor, DeliveryDto dto)
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
        
        delivery.State = dto.State;
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
                        Reference = $"Delivery {delivery.Id} update"
                    };
                }

                var diff = l.Quantity - deliveryLine.Quantity;
                return new UpdateStockDto
                {
                    ProductId = deliveryLine.ProductId,
                    Value = Math.Abs(diff),
                    Operation = diff < 0 ? StockOperation.Add : StockOperation.Remove,
                    Reference = $"Delivery {delivery.Id} update"
                };
            })
            .ToList();
        
        var result = await stockService.UpdateStockBulkAsync(user, updates);
        if (result.IsFailure)
        {
            throw new ApplicationException(result.Error);
        }
        
        unitOfWork.DeliveryRepository.RemoveRange(delivery.Lines);
        delivery.Lines = newLines;
        
        unitOfWork.DeliveryRepository.Update(delivery);
        await unitOfWork.CommitAsync();
    }

    public async Task DeleteDelivery(int id)
    {
        var delivery = await unitOfWork.DeliveryRepository.GetDeliveryById(id);
        if (delivery == null)
            throw new ApplicationException("errors.delivery-not-found");
        
        unitOfWork.DeliveryRepository.Remove(delivery);
        await unitOfWork.CommitAsync();
    }
}