using API.Data;
using API.Entities;
using API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace API.ManualMigrations;

public class ManualMigrationAddStockForExistingProducts
{
    public static async Task Migrate(DataContext ctx, ILogger<Program> logger)
    {
        if (await ctx.ManualMigrations.AnyAsync(mm => mm.Name.Equals("ManualMigrationAddStockForExistingProducts")))
        {
            return;
        }
        
        logger.LogCritical("Running ManualMigrationAddStockForExistingProducts migration - Please be patient, this may take some time. This is not an error");
        
        var products = await ctx.Products.ToListAsync();

        foreach (var product in products)
        {
            ctx.ProductStock.Add(new Stock
            {
                Product = product,
                Quantity = 0,
                History = []
            });
        }
        
        await ctx.SaveChangesAsync();
        
        await ctx.ManualMigrations.AddAsync(new ManualMigration()
        {
            Name = "ManualMigrationAddStockForExistingProducts",
            ProductVersion = BuildInfo.Version.ToString(),
            RanAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        logger.LogCritical("Running ManualMigrationAddStockForExistingProducts migration - Completed. This is not an error");
    }
}