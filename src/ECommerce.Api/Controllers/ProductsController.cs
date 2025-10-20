using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Product>>> GetProducts([FromQuery] ProductQuery query, CancellationToken ct)
    {
        var result = await _productService.GetProductsAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        if (product is null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Product>> Create(Product product, CancellationToken ct)
    {
        var created = await _productService.CreateAsync(product, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Product>> Update(Guid id, Product product, CancellationToken ct)
    {
        if (id != product.Id) return BadRequest();
        var updated = await _productService.UpdateAsync(product, ct);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _productService.DeleteAsync(id, ct);
        return NoContent();
    }
}