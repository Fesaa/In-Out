using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public interface IProductRepository
{
    Task<Product?> GetById(int id);
    Task<IList<Product>> GetByIds(IEnumerable<int> ids);
    Task<IList<ProductDto>> GetDtoByIds(IEnumerable<int> ids);
    Task<ProductCategory?> GetCategoryById(int id);
    Task<ProductCategory?> GetFirstCategory();
    Task<ProductCategory?> GetCategoryByName(string name);
    Task<IList<Product>> GetAll(bool onlyEnabled = false);
    Task<IList<ProductDto>> GetAllDto(bool onlyEnabled = false);
    Task<IList<ProductCategory>> GetAllCategories(bool onlyEnabled = false);
    Task<IList<ProductCategoryDto>> GetAllCategoriesDtos(bool onlyEnabled = false);
    Task<IList<Product>> GetByCategory(ProductCategory category);
    Task<int> GetHighestSortValue(ProductCategory category);
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

    public async Task<IList<Product>> GetByIds(IEnumerable<int> ids)
    {
        return await ctx.Products.Where(p => ids.Contains(p.Id)).ToListAsync();
    }

    public async Task<IList<ProductDto>> GetDtoByIds(IEnumerable<int> ids)
    {
        return await ctx.Products
            .Where(p => ids.Contains(p.Id))
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ProductCategory?> GetCategoryById(int id)
    {
        return await ctx.ProductCategories.Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<ProductCategory?> GetFirstCategory()
    {
        return await ctx.ProductCategories
            .OrderBy(p => p.SortValue)
            .FirstOrDefaultAsync();
    }

    public async Task<ProductCategory?> GetCategoryByName(string name)
    {
        var normalized = name.ToNormalized();
        return await ctx.ProductCategories.Where(p => p.NormalizedName == normalized).FirstOrDefaultAsync();
    }

    public async Task<IList<Product>> GetAll(bool onlyEnabled = false)
    {
        return await QueryableExtensions
            .WhereIf(ctx.Products, onlyEnabled, p => p.Enabled)
            .ToListAsync();
    }

    public async Task<IList<ProductDto>> GetAllDto(bool onlyEnabled = false)
    {
        return await QueryableExtensions
            .WhereIf(ctx.Products, onlyEnabled, p => p.Enabled)
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IList<ProductCategory>> GetAllCategories(bool onlyEnabled = false)
    {
        return await QueryableExtensions
            .WhereIf(ctx.ProductCategories, onlyEnabled, p => p.Enabled)
            .OrderBy(p => p.SortValue)
            .ToListAsync();
    }

    public async Task<IList<ProductCategoryDto>> GetAllCategoriesDtos(bool onlyEnabled = false)
    {
        return await QueryableExtensions
            .WhereIf(ctx.ProductCategories, onlyEnabled, p => p.Enabled)
            .OrderBy(p => p.SortValue)
            .ProjectTo<ProductCategoryDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IList<Product>> GetByCategory(ProductCategory category)
    {
        return await ctx.Products
            .Where(p => p.Category == category)
            .ToListAsync();
    }

    public async Task<int> GetHighestSortValue(ProductCategory category)
    {
        return await ctx.Products
            .Where(p => p.CategoryId == category.Id)
            .MaxAsync(p => p.SortValue);
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