using API.Data;
using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Entities.Enums;
using API.Helpers;
using API.Helpers.Telemetry;

namespace API.Services;

public interface IStockService
{
    
    /// <summary>
    /// Update several stocks at once, one success if all of them do
    /// </summary>
    /// <param name="user"></param>
    /// <param name="dtos"></param>
    /// <returns></returns>
    Task<Result<IList<Stock>>> UpdateStockBulkAsync(User user, IList<UpdateStockDto> dtos);
}

public class StockService(ILogger<StockService> logger, IUnitOfWork unitOfWork, ILocalizationService localization): IStockService
{

    public async Task<Result<IList<Stock>>> UpdateStockBulkAsync(User user, IList<UpdateStockDto> dtos) 
    {
        dtos = dtos.Where(dto => dto.Value != 0 || dto.Operation == StockOperation.Set).ToList();

        if (!dtos.Any())
        {
            return Result<IList<Stock>>.Failure(await localization.Translate(user.Id, "stock-bulk-empty-list"));
        }

        using var tracker = TelemetryHelper.TrackOperation("bulk_stock_update", new Dictionary<string, object?>
        {
            ["user_id"] = user.Id,
            ["update_operations"] = dtos.Count,
        });

        return await unitOfWork.ExecuteWithRetryAsync(async () =>
        {
            var results = new List<Stock>();
            var stockHistories = new List<StockHistory>();
            
            var productIds = dtos.Select(d => d.ProductId).Distinct().ToList();
            var stocks = await unitOfWork.StockRepository.GetByProductIdsAsync(productIds, StockIncludes.Product);
            var stockLookup = stocks.ToDictionary(s => s.ProductId, s => s);
            
            var missingStockIds = productIds.Where(id => !stockLookup.ContainsKey(id)).ToList();
            if (missingStockIds.Count != 0)
            {
                return Result<IList<Stock>>.Failure(
                    await localization.Translate(user.Id, "stock-bulk-stocks-not-found", string.Join(", ", missingStockIds))
                );
            }

            foreach (var dto in dtos)
            {
                var stock = stockLookup[dto.ProductId];
                var currentQ = stock.Quantity;
                
                var newQ = dto.Operation switch
                {
                    StockOperation.Add => stock.Quantity + dto.Value,
                    StockOperation.Remove => stock.Quantity - dto.Value,
                    StockOperation.Set => dto.Value,
                    _ => throw new ArgumentException($"Invalid stock operation {dto.Operation}")
                };

                if (newQ < 0)
                {
                    logger.LogWarning("{UserId} tried to update stock {StockId} to a negative value in bulk operation", 
                        user.Id, dto.ProductId);
                    return Result<IList<Stock>>.Failure(
                        await localization.Translate(user.Id, "stock-bulk-insufficient-stock", stock.Product.Name, stock.Quantity, dto.Value)
                    );
                }
                
                stock.Quantity = newQ;
                results.Add(stock);

                var history = new StockHistory
                {
                    StockId = stock.Id,
                    UserId = user.Id,
                    Operation = dto.Operation,
                    QuantityBefore = currentQ,
                    QuantityAfter = newQ,
                    Value = dto.Value,
                    Notes = dto.Notes,
                    ReferenceNumber = dto.Reference,
                    CreatedUtc = DateTime.UtcNow,
                    LastModifiedUtc = DateTime.UtcNow
                };
                
                stockHistories.Add(history);
            }
            
            unitOfWork.StockRepository.UpdateRange(results);
            unitOfWork.StockRepository.AddRange(stockHistories);
            
            logger.LogInformation("Bulk stock update completed successfully. {Count} stocks updated by {UserId}", 
                results.Count, user.Id);
            
            return Result<IList<Stock>>.Success(results);
        });
    }
    
}