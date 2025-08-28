using API.Constants;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductsController(IUnitOfWork unitOfWork, IProductService productService): BaseApiController
{

    /// <summary>
    /// Get products by ids
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpPost("by-ids")]
    public async Task<ActionResult<IList<ProductDto>>> GetProductsByIds(IList<int> ids)
    {
        return Ok(await unitOfWork.ProductRepository.GetDtoByIds(ids));
    }

    /// <summary>
    /// Retrieve all products
    /// </summary>
    /// <param name="onlyEnabled">Only return currently active products</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IList<ProductDto>>> GetProducts([FromQuery] bool onlyEnabled = false)
    {
        return Ok(await unitOfWork.ProductRepository.GetAllDto(onlyEnabled));
    }

    /// <summary>
    /// Returns all product categories
    /// </summary>
    /// <param name="onlyEnabled"></param>
    /// <returns></returns>
    [HttpGet("category")]
    public async Task<ActionResult<IList<ProductCategoryDto>>> GetProductCategories([FromQuery] bool onlyEnabled = false)
    {
        return Ok(await unitOfWork.ProductRepository.GetAllCategoriesDtos(onlyEnabled));
    }

    /// <summary>
    /// Get all products for a certain category
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IList<ProductDto>>> GetProductsByCategory(ProductCategory category)
    {
        return Ok(await unitOfWork.ProductRepository.GetByCategory(category));
    }

    /// <summary>
    /// Get a product by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        return Ok(await unitOfWork.ProductRepository.GetById(id));
    }

    /// <summary>
    /// Create a product, name must be unique
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Policy = PolicyConstants.ManageProducts)]
    public async Task<ActionResult<ProductDto>> CreateProduct(ProductDto product)
    {
        var res = await productService.CreateProduct(product);
        return Ok(res);
    }

    /// <summary>
    /// Create a new product category, name must be unique
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpPost("category")]
    [Authorize(Policy = PolicyConstants.ManageProducts)]
    public async Task<ActionResult<ProductCategoryDto>> CreateCategory(ProductCategoryDto category)
    {
        var res = await productService.CreateProductCategory(category);
        return Ok(res);
    }

    /// <summary>
    /// Update a product, will error if the product id is not found. Name must be unique
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    [HttpPut]
    [Authorize(Policy = PolicyConstants.ManageProducts)]
    public async Task<IActionResult> UpdateProduct(ProductDto product)
    {
        await productService.UpdateProduct(product);
        return Ok();
    }

    /// <summary>
    /// Update a product category, will error if the category id is not found. Name must be unique
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpPut("category")]
    [Authorize(Policy = PolicyConstants.ManageProducts)]
    public async Task<IActionResult> UpdateCategory(ProductCategoryDto category)
    {
        await productService.UpdateProductCategory(category);
        return Ok();
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = PolicyConstants.ManageProducts)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await productService.DeleteProduct(id);
        return Ok();
    }

    /// <summary>
    /// Delete a product category
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("category/{id}")]
    [Authorize(Policy = PolicyConstants.ManageProducts)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await productService.DeleteProductCategory(id);
        return Ok();
    }
    
    /// <summary>
    /// Re-order categories
    /// </summary>
    /// <param name="ids">Ids of the categories in the wanted order</param>
    /// <returns></returns>
    [HttpPost("category/order")]
    [Authorize(Policy = PolicyConstants.ManageProducts)]
    public async Task<IActionResult> OrderCategories(IList<int> ids)
    {
        await productService.OrderCategories(ids);
        return Ok();
    }
}
