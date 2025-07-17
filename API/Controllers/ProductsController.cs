using API.Data;
using API.DTOs;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductsController(IUnitOfWork unitOfWork, IProductService productService): BaseApiController
{

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
    public async Task<ActionResult<ProductDto>> CreateProduct(ProductDto product)
    {
        try
        {
            var res = await productService.CreateProduct(product);
            return Ok(res);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Create a new product category, name must be unique
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpPost("category")]
    public async Task<ActionResult<ProductCategoryDto>> CreateCategory(ProductCategoryDto category)
    {
        try
        {
            var res = await productService.CreateProductCategory(category);
            return Ok(res);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update a product, will error if the product id is not found. Name must be unique
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> UpdateProduct(ProductDto product)
    {
        try
        {
            await productService.UpdateProduct(product);
            return Ok();
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update a product category, will error if the category id is not found. Name must be unique
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpPut("category")]
    public async Task<IActionResult> UpdateCategory(ProductCategoryDto category)
    {
        try
        {
            await productService.UpdateProductCategory(category);
            return Ok();
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            await productService.DeleteProduct(id);
            return Ok();
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a product category
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("category/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            await productService.DeleteProductCategory(id);
            return Ok();
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("category/order")]
    public async Task<IActionResult> OrderCategories(IList<int> ids)
    {
        try
        {
            await productService.OrderCategories(ids);
            return Ok();
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
