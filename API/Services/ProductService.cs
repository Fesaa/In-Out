using API.Data;
using API.DTOs;
using API.Entities;
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
}

public class ProductService(IUnitOfWork unitOfWork, IMapper mapper): IProductService
{
    public async Task<ProductDto> CreateProduct(ProductDto dto)
    {
        var other = await unitOfWork.ProductRepository.GetByName(dto.Name);
        if (other != null) throw new ApplicationException("errors.name-in-use");

        var product = new Product
        {
            Name = dto.Name,
            NormalizedName = dto.Name.ToNormalized(),
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Type = dto.Type,
            IsTracked = dto.IsTracked,
            Enabled = dto.Enabled,
        };

        unitOfWork.ProductRepository.Add(product);
        await unitOfWork.CommitAsync();

        return mapper.Map<ProductDto>(product);
    }

    public async Task<ProductCategoryDto> CreateProductCategory(ProductCategoryDto dto)
    {
        var extCategory = await unitOfWork.ProductRepository.GetCategoryByName(dto.Name);
        if (extCategory != null) throw new ApplicationException("errors.name-in-use");

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
        if (extProduct == null) throw new ApplicationException("errors.product-not-found");

        if (extProduct.NormalizedName != dto.Name.ToNormalized())
        {
            var other = await unitOfWork.ProductRepository.GetByName(dto.Name);
            if (other != null) throw new ApplicationException("errors.name-in-use");

            extProduct.Name = dto.Name;
            extProduct.NormalizedName = dto.Name.ToNormalized();
        }

        extProduct.Description = dto.Description;
        extProduct.CategoryId = dto.CategoryId;
        extProduct.Type = dto.Type;
        extProduct.IsTracked = dto.IsTracked;
        extProduct.Enabled  = dto.Enabled;

        unitOfWork.ProductRepository.Update(extProduct);
        if (unitOfWork.HasChanges())
        {
            await unitOfWork.CommitAsync();
        }
    }

    public async Task UpdateProductCategory(ProductCategoryDto dto)
    {
        var category = await unitOfWork.ProductRepository.GetCategoryById(dto.Id);
        if (category == null) throw new ApplicationException("errors.product-not-found");

        if (category.NormalizedName != dto.Name.ToNormalized())
        {
            var other = await unitOfWork.ProductRepository.GetCategoryByName(dto.Name);
            if (other != null) throw new ApplicationException("errors.name-in-use");

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
        if (product == null) throw new ApplicationException("errors.product-not-found");

        unitOfWork.ProductRepository.Delete(product);
        await unitOfWork.CommitAsync();
    }
    public async Task DeleteProductCategory(int id)
    {
        var category = await unitOfWork.ProductRepository.GetCategoryById(id);
        if (category == null) throw new ApplicationException("errors.product-not-found");

        unitOfWork.ProductRepository.Delete(category);
        await unitOfWork.CommitAsync();
    }
}