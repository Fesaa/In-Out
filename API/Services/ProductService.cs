using API.Data;
using API.DTOs;
using API.Entities;
using API.Exceptions;
using API.Extensions;
using AutoMapper;

namespace API.Services;

public interface IProductService
{
    Task<ProductDto> CreateProduct(ProductDto dto);
    Task<ProductCategoryDto> CreateProductCategory(ProductCategoryDto dto);
    Task UpdateProduct(ProductDto dto);
    Task UpdateProductCategory(ProductCategoryDto dto);
    Task DeleteProduct(int id);
    Task DeleteProductCategory(int id);
    Task<PriceCategoryDto> CreatePriceCategory(PriceCategoryDto dto);
    Task UpdatePriceCategory(PriceCategoryDto dto);

    /// <summary>
    /// Sets the sort value of all categories to the index in the list
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task OrderCategories(IList<int> ids);
    /// <summary>
    /// Orders produtcs inside a category
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task OrderProducts(IList<int> ids);
}

public class ProductService(IUnitOfWork unitOfWork, IMapper mapper): IProductService
{
    public async Task<ProductDto> CreateProduct(ProductDto dto)
    {
        var category = await unitOfWork.ProductRepository.GetCategoryById(dto.CategoryId);
        if (category == null) throw new InOutException("errors.category-not-found");

        var prices = await ValidateAndMapPrices(dto.Prices);

        var maxSortValue = await unitOfWork.ProductRepository.GetHighestSortValue(category);

        var product = new Product
        {
            Name = dto.Name,
            NormalizedName = dto.Name.ToNormalized(),
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Type = dto.Type,
            IsTracked = dto.IsTracked,
            Enabled = dto.Enabled,
            SortValue = maxSortValue + 1,
            Prices = prices
        };

        unitOfWork.ProductRepository.Add(product);
        await unitOfWork.CommitAsync();

        var stock = new Stock { Product = product, Quantity = 0 };
        unitOfWork.StockRepository.Add(stock);
        await unitOfWork.CommitAsync();

        return mapper.Map<ProductDto>(product);
    }

    public async Task<ProductCategoryDto> CreateProductCategory(ProductCategoryDto dto)
    {
        var extCategory = await unitOfWork.ProductRepository.GetCategoryByName(dto.Name);
        if (extCategory != null) throw new InOutException("errors.name-in-use");

        var category = new ProductCategory
        {
            Name = dto.Name,
            NormalizedName = dto.Name.ToNormalized(),
            AutoCollapse = dto.AutoCollapse,
            Enabled = dto.Enabled,
            SortValue = dto.SortValue,
        };

        unitOfWork.ProductRepository.Add(category);
        await unitOfWork.CommitAsync();

        return mapper.Map<ProductCategoryDto>(category);
    }

    public async Task UpdateProduct(ProductDto dto)
    {
        var extProduct = await unitOfWork.ProductRepository.GetById(dto.Id);
        if (extProduct == null) throw new InOutException("errors.product-not-found");

        if (extProduct.NormalizedName != dto.Name.ToNormalized())
        {
            extProduct.Name = dto.Name;
            extProduct.NormalizedName = dto.Name.ToNormalized();
        }

        if (extProduct.CategoryId != dto.CategoryId)
        {
            var category = await unitOfWork.ProductRepository.GetCategoryById(dto.CategoryId);
            if (category == null) throw new InOutException("errors.category-not-found");

            var maxSortValue = await unitOfWork.ProductRepository.GetHighestSortValue(category);
            extProduct.CategoryId = dto.CategoryId;
            extProduct.SortValue = maxSortValue + 1;
        }

        extProduct.Prices = await ValidateAndMapPrices(dto.Prices);

        extProduct.Description = dto.Description;
        extProduct.Type = dto.Type;
        extProduct.IsTracked = dto.IsTracked;
        extProduct.Enabled = dto.Enabled;

        unitOfWork.ProductRepository.Update(extProduct);
        if (unitOfWork.HasChanges()) await unitOfWork.CommitAsync();
    }

    public async Task UpdateProductCategory(ProductCategoryDto dto)
    {
        var category = await unitOfWork.ProductRepository.GetCategoryById(dto.Id);
        if (category == null) throw new InOutException("errors.product-not-found");

        if (category.NormalizedName != dto.Name.ToNormalized())
        {
            var other = await unitOfWork.ProductRepository.GetCategoryByName(dto.Name);
            if (other != null) throw new InOutException("errors.name-in-use");

            category.Name = dto.Name;
            category.NormalizedName = dto.Name.ToNormalized();
        }

        category.Enabled = dto.Enabled;
        category.AutoCollapse = dto.AutoCollapse;
        category.SortValue = dto.SortValue;

        unitOfWork.ProductRepository.Update(category);

        if (unitOfWork.HasChanges())
            await unitOfWork.CommitAsync();
    }

    public async Task DeleteProduct(int id)
    {
        var product = await unitOfWork.ProductRepository.GetById(id);
        if (product == null) throw new InOutException("errors.product-not-found");

        unitOfWork.ProductRepository.Delete(product);
        await unitOfWork.CommitAsync();
    }
    public async Task DeleteProductCategory(int id)
    {
        var category = await unitOfWork.ProductRepository.GetCategoryById(id);
        if (category == null) throw new InOutException("errors.product-not-found");

        var products = await unitOfWork.ProductRepository.GetByCategory(category);
        if (products.Count > 0)
        {
            var defaultCategory = await unitOfWork.ProductRepository.GetFirstCategory();
            if (defaultCategory == null || defaultCategory.Id == category.Id)
                throw new InOutException("errors.no-fallback-category");

            foreach (var product in products)
            {
                product.Category = defaultCategory;
                unitOfWork.ProductRepository.Update(product);
            }
        }

        unitOfWork.ProductRepository.Delete(category);
        await unitOfWork.CommitAsync();
    }

    public async Task<PriceCategoryDto> CreatePriceCategory(PriceCategoryDto dto)
    {
        var existing = await unitOfWork.ProductRepository.GetPriceCategoryByName(dto.Name);
        if (existing != null) throw new InOutException("errors.price-category-exists");

        var priceCategory = new PriceCategory
        {
            Name = dto.Name,
            NormalizedName = dto.Name.ToNormalized()
        };

        unitOfWork.ProductRepository.Add(priceCategory);
        await unitOfWork.CommitAsync();

        return mapper.Map<PriceCategoryDto>(priceCategory);
    }

    public async Task UpdatePriceCategory(PriceCategoryDto dto)
    {
        var priceCategory = await unitOfWork.ProductRepository.GetPriceCategoryById(dto.Id);
        if (priceCategory == null) throw new InOutException("errors.price-category-not-found");

        if (priceCategory.NormalizedName != dto.Name.ToNormalized())
        {
            var other = await unitOfWork.ProductRepository.GetPriceCategoryByName(dto.Name);
            if (other != null) throw new InOutException("errors.name-in-use");

            priceCategory.Name = dto.Name;
            priceCategory.NormalizedName = dto.Name.ToNormalized();
        }

        unitOfWork.ProductRepository.Update(priceCategory);
        if (unitOfWork.HasChanges()) await unitOfWork.CommitAsync();
    }

    public async Task OrderCategories(IList<int> ids)
    {
        ids = ids.Distinct().ToList();
        var categories = await unitOfWork.ProductRepository.GetAllCategories();
        if (ids.Count != categories.Count) throw new InOutException("errors.not-enough-categories");

        foreach (var category in categories)
        {
            category.SortValue = ids.IndexOf(category.Id);
            unitOfWork.ProductRepository.Update(category);
        }

        await unitOfWork.CommitAsync();
    }

    public async Task OrderProducts(IList<int> ids)
    {
        ids = ids.Distinct().ToList();
        var products = await unitOfWork.ProductRepository.GetByIds(ids);
        if (ids.Count != products.Count) throw new InOutException("errors.not-enough-products");

        if (products.Select(p => p.CategoryId).Distinct().Count() != 1)
        {
            throw new InOutException("errors.no-sorting-between-categories");
        }

        var category = await unitOfWork.ProductRepository.GetCategoryById(products.First().CategoryId);
        if (category == null) throw new InOutException("errors.category-not-found");

        var allProducts = await unitOfWork.ProductRepository.GetByCategory(category);
        if (allProducts.Count != products.Count) throw new InOutException("errors.product-not-found");

        foreach (var product in products)
        {
            product.SortValue = ids.IndexOf(product.Id);
            unitOfWork.ProductRepository.Update(product);
        }

        await unitOfWork.CommitAsync();
    }

    /// <summary>
    /// Validates that all PriceCategory IDs in the DTO exist in the database.
    /// </summary>
    private async Task<Dictionary<int, float>> ValidateAndMapPrices(Dictionary<int, float> dtoPrices)
    {
        if (dtoPrices == null || !dtoPrices.Any()) return new Dictionary<int, float>();

        var allPriceCategories = await unitOfWork.ProductRepository.GetAllPriceCategories();
        var validIds = allPriceCategories.Select(pc => pc.Id).ToHashSet();
        var result = new Dictionary<int, float>();

        foreach (var entry in dtoPrices)
        {
            // We check the ID from the PriceCategoryDto key
            if (!validIds.Contains(entry.Key))
                throw new InOutException($"errors.price-category-not-found-id-{entry.Key}");

            result.Add(entry.Key, entry.Value);
        }

        return result;
    }
}
