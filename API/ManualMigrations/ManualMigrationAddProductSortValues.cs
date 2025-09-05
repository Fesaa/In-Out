using API.Data;
using API.Entities;
using API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace API.ManualMigrations;

public static class ManualMigrationAddProductSortValues
{

    public static async Task Migrate(DataContext ctx, ILogger<Program> logger)
    {
        if (await ctx.ManualMigrations.AnyAsync(mm => mm.Name.Equals("ManualMigrationAddProductSortValues")))
        {
            return;
        }
        
        logger.LogCritical("Running ManualMigrationAddProductSortValues migration - Please be patient, this may take some time. This is not an error");
        
        
        var products = await ctx.Products.ToListAsync();
        var productsByCategory = products.GroupBy(p => p.CategoryId);
        
        foreach (var grouping in productsByCategory)
        {
            var idx = 0;
            using var iter = grouping.OrderBy(p => p.NormalizedName).GetEnumerator();
            while (iter.MoveNext())
            {
                var product = iter.Current;
                product.SortValue = idx++;
            }
        }
        
        await ctx.SaveChangesAsync();
        
        await ctx.ManualMigrations.AddAsync(new ManualMigration
        {
            Name = "ManualMigrationAddProductSortValues",
            ProductVersion = BuildInfo.Version.ToString(),
            RanAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        logger.LogCritical("Running ManualMigrationAddProductSortValues migration - Completed. This is not an error");
        
    }
    
}