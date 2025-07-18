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
    Task CreateDelivery(int userId, DeliveryDto  dto);
    Task UpdateDelivery(ClaimsPrincipal actor, DeliveryDto dto);
    Task DeleteDelivery(int id);
}

public class DeliveryService(IUnitOfWork unitOfWork): IDeliveryService
{
    public static readonly IList<DeliveryState> FinalDeliveryStates = [DeliveryState.Completed, DeliveryState.Canceled];

    public async Task CreateDelivery(int userId, DeliveryDto dto)
    {
        var client = await unitOfWork.ClientRepository.GetClientById(dto.ClientId);
        if (client == null)
            throw new ApplicationException("errors.client-not-found");

        // Ensure no duplicates
        var dtoLines = dto.Lines.GroupBy(l => l.ProductId)
            .Select(g => new DeliveryLineDto
            {
                ProductId = g.Key,
                Quantity = g.Sum(l => l.Quantity),
            });

        var lines = new List<DeliveryLine>();
        foreach (var deliveryLineDto in dtoLines)
        {
            var line = new DeliveryLine
            {
                ProductId = deliveryLineDto.ProductId,
                Quantity = deliveryLineDto.Quantity,
            };

            await EnsureEnoughSpace(line.ProductId, line.Quantity);

            lines.Add(line);
        }
        
        var delivery = new Delivery
        {
            State = DeliveryState.InProgress,
            UserId = userId,
            Recipient = client,
            Message = dto.Message.Trim(),
            Lines = lines,
        };
        
        unitOfWork.DeliveryRepository.Add(delivery);
        await unitOfWork.CommitAsync();
    }

    public async Task UpdateDelivery(ClaimsPrincipal actor, DeliveryDto dto)
    {
        var delivery = await unitOfWork.DeliveryRepository.GetDeliveryById(dto.Id);
        if (delivery == null)
            throw new ApplicationException("errors.delivery-not-found");
        
        if (FinalDeliveryStates.Contains(delivery.State))
            throw new ApplicationException("errors.delivery-locked");
        
        if (delivery.Recipient.Id != dto.ClientId)
            throw new ApplicationException("errors.cannot-change-recipient");
        
        var user = await unitOfWork.UsersRepository.GetByUserIdAsync(actor.GetUserId());
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
            });
        
        var linesLookup = delivery.Lines.ToDictionary(l => l.ProductId, l => l);
        
        var newLines = new List<DeliveryLine>();
        foreach (var deliveryLineDto in dtoLines)
        {
            if (linesLookup.TryGetValue(deliveryLineDto.ProductId, out var deliveryLine))
            {
                var diff = deliveryLineDto.Quantity - deliveryLine.Quantity;
                await EnsureEnoughSpace(deliveryLine.ProductId, diff);
            }
            else
            {
                await EnsureEnoughSpace(deliveryLineDto.ProductId, deliveryLineDto.Quantity);
            }

            newLines.Add(new DeliveryLine
            {
                ProductId = deliveryLineDto.ProductId,
                Quantity = deliveryLineDto.Quantity,
            });
        }
        
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

    /// <summary>
    /// Checks if the stock can take the change in amount. Updates it if possible, and throws if not
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="amount"></param>
    /// <param name="force"></param>
    private async Task EnsureEnoughSpace(int productId, int amount, bool force = false)
    {
        // TODO: Check with stock
    }
}