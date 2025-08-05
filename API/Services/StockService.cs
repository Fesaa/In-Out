using API.Data;
using API.DTOs;
using API.Entities;
using API.Entities.Enums;
using API.Extensions;
using API.Helpers;

namespace API.Services;

public interface IStockService
{
    Task UpdateStockAsync(StockDto dto);
    
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
    public async Task UpdateStockAsync(StockDto dto)
    {
        var stock = await unitOfWork.StockRepository.GetByIdAsync(dto.Id);
        if (stock == null)
        {
            throw new ApplicationException("errors.stock-not-found");
        }

        if (stock.Name.ToNormalized() != dto.Name.ToNormalized())
        {
            stock.Name = dto.Name;
        }

        if (stock.Description.ToNormalized() != dto.Description.ToNormalized())
        {
            stock.Description = dto.Description;
        }

        if (stock.ProductId != dto.ProductId)
        {
            var product = await unitOfWork.ProductRepository.GetById(dto.ProductId);
            if (product == null)
            {
                throw new ApplicationException("errors.product-not-found");
            }
            
            var other = await unitOfWork.StockRepository.GetDtoForProduct(dto.ProductId);
            if (other != null)
            {
                // TODO: Support several stocks per product; Need to update delivery 
                throw new ApplicationException("errors.product-in-use");
            }
            
            stock.ProductId = product.Id;
            stock.Product = product;
        }

        if (unitOfWork.HasChanges())
        {
            await unitOfWork.CommitAsync();
        }
    }

    public async Task<Result<IList<Stock>>> UpdateStockBulkAsync(User user, IList<UpdateStockDto> dtos) 
    {
        if (dtos == null || !dtos.Any())
        {
            return Result<IList<Stock>>.Failure(await localization.Translate(user.Id, "stock-bulk-empty-list"));
        }
        
        dtos = dtos.Where(dto => dto.Value != 0).ToList();

        return await unitOfWork.ExecuteWithRetryAsync(async () =>
        {
            var results = new List<Stock>();
            var stockHistories = new List<StockHistory>();
            
            var stockIds = dtos.Select(d => d.ProductId).Distinct().ToList();
            var stocks = await unitOfWork.StockRepository.GetByIdsAsync(stockIds);
            var stockLookup = stocks.ToDictionary(s => s.Id, s => s);
            
            var missingStockIds = stockIds.Where(id => !stockLookup.ContainsKey(id)).ToList();
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
                    logger.LogWarning("{UserName} tried to update stock {StockId} to a negative value in bulk operation", 
                        user.Name, dto.ProductId);
                    return Result<IList<Stock>>.Failure(
                        await localization.Translate(user.Id, "stock-bulk-insufficient-stock", dto.ProductId, stock.Quantity, dto.Value)
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
            
            logger.LogInformation("Bulk stock update completed successfully. {Count} stocks updated by {UserName}", 
                results.Count, user.Name);
            
            return Result<IList<Stock>>.Success(results);
        });
    }
    
}