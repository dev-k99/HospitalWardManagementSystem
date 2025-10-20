using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken ct)
    {
        var items = await _cartService.GetCartAsync(GetUserId(), ct);
        return Ok(items);
    }

    [HttpPost("add")] 
    public async Task<IActionResult> Add(Guid productId, int quantity, CancellationToken ct)
    {
        await _cartService.AddToCartAsync(GetUserId(), productId, quantity, ct);
        return Ok();
    }

    [HttpPost("update")] 
    public async Task<IActionResult> Update(Guid productId, int quantity, CancellationToken ct)
    {
        await _cartService.UpdateQuantityAsync(GetUserId(), productId, quantity, ct);
        return Ok();
    }

    [HttpPost("remove")] 
    public async Task<IActionResult> Remove(Guid productId, CancellationToken ct)
    {
        await _cartService.RemoveFromCartAsync(GetUserId(), productId, ct);
        return Ok();
    }
}