using System.Net;
using System.Security.Claims;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : Controller
{
    private readonly IProductsManagementService _service;

    public ProductController(IProductsManagementService service)
    {
        _service = service;
    }

    [HttpGet()]
    [AllowAnonymous]
    //public async Task<IActionResult> GetProducts()
    //{
    //    var products = await _service.GetProducts(request);
    //    return Ok(products);
    //}

    public async Task<IActionResult> GetProducts([FromQuery] ProductModel.FilterProduct request)
    {
        var products = await _service.GetProducts(request);

        if (products == null)
        {
            Response.Headers.Append("X-Message", "No hay productos activos");
            return NoContent();
        }

        return Ok(products);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAuthProducts([FromQuery] ProductModel.FilterAuthProduct request)
    {
        var products = await _service.GetAuthProducts(request);

        if (products == null)
        {
            Response.Headers.Append("X-Message", "No hay productos");
            return NoContent();
        }
        
        return Ok(products);
    }


    [HttpGet("{id}")]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _service.GetProductById(id);
        return Ok(product);
    }

    [HttpPost()]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> AddProduct([FromBody] ProductModel.RequestProduct request)
    {
        var product = await _service.AddProduct(request);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateProductAsync(Guid id, [FromBody] ProductModel.RequestProduct request)
    {
        var updatedProduct = await _service.UpdateProduct(id, request);
        return Ok(updatedProduct);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeactivateProductAsync(Guid id)
    {
        var deactivatedProduct = await _service.DeactivateProduct(id);
        if (deactivatedProduct == null)
        { 
            return NotFound();
        }
        return NoContent();
    }
}

