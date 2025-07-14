using API.Entities;
using API.Entities.Enums;
using API.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public interface IProductRepository
{
    Task<Product?> GetById(int id);
    Task<Product?> GetByName(string name);
    Task<IList<Product>> GetAll(bool onlyEnabled = false);
    Task<IList<Product>> GetByCategory(ProductCategory category);
    void Add(Product product);
    void Add(ProductCategory category);
    void Update(Product product);
    void Update(ProductCategory category);
    void Delete(Product product);
    void Delete(ProductCategory category);
    Task<int> Count();
}

public class ProductRepository(DataContext ctx, IMapper mapper): IProductRepository
{

    public async Task<Product?> GetById(int id)
    {
        return await ctx.Products.Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public Task<Product?> GetByName(string name)
    {
        var normalized = name.ToNormalized();
        return ctx.Products.Where(p => p.NormalizedName == normalized).FirstOrDefaultAsync();
    }

    public async Task<IList<Product>> GetAll(bool onlyEnabled = false)
    {
        return await ctx.Products
            .WhereIf(onlyEnabled, p => p.Enabled)
            .ToListAsync();
    }

    public async Task<IList<Product>> GetByCategory(ProductCategory category)
    {
        return await ctx.Products
            .Where(p => p.Category == category)
            .ToListAsync();
    }

    public void Add(Product product)
    {
        ctx.Products.Add(product).State = EntityState.Added;
    }

    public void Add(ProductCategory category)
    {
        ctx.ProductCategories.Add(category).State = EntityState.Added;
    }

    public void Update(Product product)
    {
        ctx.Products.Update(product).State = EntityState.Modified;
    }

    public void Update(ProductCategory category)
    {
        ctx.ProductCategories.Update(category).State = EntityState.Modified;
    }

    public void Delete(Product product)
    {
        ctx.Products.Remove(product).State = EntityState.Deleted;
    }

    public void Delete(ProductCategory category)
    {
        ctx.ProductCategories.Remove(category).State = EntityState.Deleted;
    }

    public async Task<int> Count()
    {
        return await ctx.Products.CountAsync();
    }
}