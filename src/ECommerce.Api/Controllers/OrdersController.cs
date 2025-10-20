using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;

    public OrdersController(IOrderService orderService, ICartService cartService)
    {
        _orderService = orderService;
        _cartService = cartService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Create(string? stripePaymentId, CancellationToken ct)
    {
        var userId = GetUserId();
        var cart = await _cartService.GetCartAsync(userId, ct);
        var order = await _orderService.CreateOrderAsync(userId, cart, stripePaymentId, ct);
        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders(CancellationToken ct)
    {
        var orders = await _orderService.GetOrdersForUserAsync(GetUserId(), ct);
        return Ok(orders);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> All(CancellationToken ct)
    {
        var orders = await _orderService.GetAllOrdersAsync(ct);
        return Ok(orders);
    }
}