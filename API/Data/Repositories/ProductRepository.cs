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
    Task<PriceCategory?> GetPriceCategoryById(int id);
    Task<PriceCategory?> GetPriceCategoryByName(string name);
    Task<IList<PriceCategory>> GetAllPriceCategories();
    Task<IList<PriceCategoryDto>> GetAllPriceCategoryDtos();
    void Add(Product product);
    void Add(ProductCategory category);
    void Add(PriceCategory priceCategory);
    void Update(Product product);
    void Update(ProductCategory category);
    void Update(PriceCategory priceCategory);
    void Delete(Product product);
    void Delete(ProductCategory category);
    void Delete(PriceCategory priceCategory);
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
        try
        {
            return await ctx.Products
                .Where(p => p.CategoryId == category.Id)
                .MaxAsync(p => p.SortValue);
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    public Task<PriceCategory?> GetPriceCategoryById(int id)
    {
        return ctx.PriceCategories.FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task<PriceCategory?> GetPriceCategoryByName(string name)
    {
        var normalized = name.ToNormalized();
        return ctx.PriceCategories.FirstOrDefaultAsync(p => p.NormalizedName == normalized);
    }

    public async Task<IList<PriceCategory>> GetAllPriceCategories()
    {
        return await ctx.PriceCategories.ToListAsync();
    }

    public async Task<IList<PriceCategoryDto>> GetAllPriceCategoryDtos()
    {
        return await ctx.PriceCategories
            .ProjectTo<PriceCategoryDto>(mapper.ConfigurationProvider)
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

    public void Add(PriceCategory priceCategory)
    {
        ctx.PriceCategories.Add(priceCategory).State = EntityState.Added;
    }

    public void Update(Product product)
    {
        ctx.Products.Update(product).State = EntityState.Modified;
    }

    public void Update(ProductCategory category)
    {
        ctx.ProductCategories.Update(category).State = EntityState.Modified;
    }

    public void Update(PriceCategory priceCategory)
    {
        ctx.PriceCategories.Update(priceCategory).State = EntityState.Modified;
    }

    public void Delete(Product product)
    {
        ctx.Products.Remove(product).State = EntityState.Deleted;
    }

    public void Delete(ProductCategory category)
    {
        ctx.ProductCategories.Remove(category).State = EntityState.Deleted;
    }

    public void Delete(PriceCategory priceCategory)
    {
        ctx.PriceCategories.Remove(priceCategory).State = EntityState.Deleted;
    }

    public async Task<int> Count()
    {
        return await ctx.Products.CountAsync();
    }
}
