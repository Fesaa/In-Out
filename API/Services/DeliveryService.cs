using System.Security.Claims;
using API.Constants;
using API.Data;
using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Entities.Enums;
using API.Exceptions;
using API.Extensions;
using API.Helpers.Telemetry;

namespace API.Services;

public interface IDeliveryService
{
    Task<Delivery> CreateDelivery(int userId, DeliveryDto  dto);
    Task<Delivery> UpdateDelivery(ClaimsPrincipal actor, DeliveryDto dto);
    Task DeleteDelivery(ClaimsPrincipal actor, int id);
    Task TransitionDelivery(ClaimsPrincipal actor, int deliveryId, DeliveryState nextState);
}

public class DeliveryService(ILogger<DeliveryService> logger, IUnitOfWork unitOfWork, IUserService userService, IStockService stockService): IDeliveryService
{
    private static readonly IList<DeliveryState> FinalDeliveryStates = [DeliveryState.Completed, DeliveryState.Cancelled, DeliveryState.Handled];

    public async Task<Delivery> CreateDelivery(int userId, DeliveryDto dto)
    {
        var user = await unitOfWork.UsersRepository.GetByUserIdAsync(userId) ?? throw new InOutException("errors.client-not-found");
        var client = await unitOfWork.ClientRepository.GetClientById(dto.ClientId) ?? throw new InOutException("errors.client-not-found");

        using var tracker = TelemetryHelper.TrackOperation("create_delivery", new Dictionary<string, object?> { ["user_id"] = userId });

        var delivery = new Delivery
        {
            State = DeliveryState.InProgress,
            UserId = userId,
            Recipient = client,
            Message = dto.Message?.Trim() ?? string.Empty,
            Lines = [],
            SystemMessages = [],
        };

        // For creation, we reconcile against an empty list
        await ReconcileStockAndLines(user, delivery, dto.Lines, isNew: true);

        unitOfWork.DeliveryRepository.Add(delivery);
        await unitOfWork.CommitAsync();

        return delivery;
    }

    public async Task<Delivery> UpdateDelivery(ClaimsPrincipal actor, DeliveryDto dto)
    {
        var delivery = await unitOfWork.DeliveryRepository.GetDeliveryById(dto.Id, DeliveryIncludes.Complete)
            ?? throw new InOutException("errors.delivery-not-found");

        if (FinalDeliveryStates.Contains(delivery.State))
            throw new InOutException("errors.delivery-locked");

        if (delivery.Recipient.Id != dto.ClientId)
            throw new InOutException("errors.cannot-change-recipient");

        var currentUser = await userService.GetUser(actor);
        if (delivery.UserId != currentUser.Id && !actor.IsInRole(PolicyConstants.CreateForOthers))
            throw new UnauthorizedAccessException();

        using var tracker = TelemetryHelper.TrackOperation("update_delivery", new Dictionary<string, object?> { ["user_id"] = currentUser.Id });

        delivery.Message = dto.Message;
        if (delivery.UserId != dto.FromId)
        {
            if (currentUser.Id != dto.FromId && !actor.IsInRole(PolicyConstants.CreateForOthers))
                throw new UnauthorizedAccessException();
            delivery.UserId = dto.FromId;
        }

        await ReconcileStockAndLines(currentUser, delivery, dto.Lines, isNew: false);

        unitOfWork.DeliveryRepository.Update(delivery);
        await unitOfWork.CommitAsync();

        return delivery;
    }

    private async Task ReconcileStockAndLines(User actor, Delivery delivery, IEnumerable<DeliveryLineDto> newLinesDto, bool isNew)
    {
        var aggregatedNewLines = newLinesDto
            .GroupBy(l => l.ProductId)
            .Select(g => new DeliveryLine { ProductId = g.Key, Quantity = g.Sum(l => l.Quantity) })
            .ToList();

        var existingLinesLookup = delivery.Lines.ToDictionary(l => l.ProductId, l => l.Quantity);
        var productIds = aggregatedNewLines.Select(l => l.ProductId).Union(existingLinesLookup.Keys).ToList();
        var productLookup = (await unitOfWork.ProductRepository.GetByIds(productIds)).ToDictionary(p => p.Id, p => p);

        var stockUpdates = new List<UpdateStockDto>();
        foreach (var productId in productIds)
        {
            if (!productLookup.TryGetValue(productId, out var product) || !product.IsTracked) continue;

            existingLinesLookup.TryGetValue(productId, out var oldQty);
            var newQty = aggregatedNewLines.FirstOrDefault(l => l.ProductId == productId)?.Quantity ?? 0;

            var diff = newQty - oldQty;
            if (diff == 0) continue;

            // Logically: Positive diff means we are delivering MORE (Remove from stock)
            // Negative diff means we reduced the delivery (Add back to stock)
            var operation = diff > 0 ? StockOperation.Remove : StockOperation.Add;
            var actionText = diff > 0 ? "Adjustment (Out)" : "Adjustment (In)";
            var reference = isNew
                ? $"New Delivery to {delivery.Recipient.Name} by {actor.Name}"
                : $"Delivery {delivery.Id} update by {actor.Name}: {actionText}";

            stockUpdates.Add(new UpdateStockDto
            {
                ProductId = productId,
                Value = Math.Abs(diff),
                Operation = operation,
                Reference = reference
            });
        }

        if (stockUpdates.Count > 0)
        {
            var result = await stockService.UpdateStockBulkAsync(actor, stockUpdates);
            if (result.IsFailure) throw new InOutException(result.Error);
        }

        if (!isNew) unitOfWork.DeliveryRepository.RemoveRange(delivery.Lines);
        delivery.Lines = aggregatedNewLines;
    }

    public async Task DeleteDelivery(ClaimsPrincipal actor, int id)
    {
        var delivery = await unitOfWork.DeliveryRepository.GetDeliveryById(id, DeliveryIncludes.Complete);
        if (delivery == null)
            throw new InOutException("errors.delivery-not-found");

        await RefundDelivery(await userService.GetUser(actor), delivery);

        unitOfWork.DeliveryRepository.Remove(delivery);
        await unitOfWork.CommitAsync();
    }

    public async Task TransitionDelivery(ClaimsPrincipal actor, int deliveryId, DeliveryState nextState)
    {
        var user = await userService.GetUser(actor);

        var canHandleDeliveries = actor.IsInRole(PolicyConstants.HandleDeliveries);

        var delivery = await unitOfWork.DeliveryRepository.GetDeliveryById(deliveryId, DeliveryIncludes.Complete);
        if (delivery == null)
            throw new InOutException("errors.delivery-not-found");

        var validNextStates = GetStateOptions(delivery.State, canHandleDeliveries);
        if (!validNextStates.Contains(nextState))
            throw new InOutException("errors.invalid-next-state");

        delivery.State = nextState;

        if (delivery.State == DeliveryState.Cancelled)
        {
            await RefundDelivery(user, delivery);
        }

        await unitOfWork.CommitAsync();
    }

    /// <summary>
    /// Return all items to stock
    /// </summary>
    /// <param name="user"></param>
    /// <param name="delivery">Load <see cref="DeliveryIncludes.Complete"/></param>
    private async Task RefundDelivery(User user, Delivery delivery)
    {
        var reference = $"Automatic refund after delivery cancellation by {user.Name} ";
        var note = $"Original delivery by {delivery.From.Name} to {delivery.Recipient.Name}";

        await stockService.UpdateStockBulkAsync(user, delivery.Lines.Select(l => new UpdateStockDto
        {
            ProductId = l.ProductId,
            Value = l.Quantity,
            Operation = StockOperation.Add,
            Reference = reference,
            Notes = note,
        }).ToList());
    }

    private static List<DeliveryState> GetStateOptions(DeliveryState currentState, bool canHandleDeliveries)
    {
        switch (currentState)
        {
            case DeliveryState.InProgress:
                return [DeliveryState.Completed, DeliveryState.Cancelled];

            case DeliveryState.Completed:
                var states = new List<DeliveryState>
                {
                    DeliveryState.Cancelled,
                };

                if (canHandleDeliveries)
                {
                    states.Add(DeliveryState.InProgress);
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
