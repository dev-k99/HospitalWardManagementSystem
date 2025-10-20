using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("intent")]
    [Authorize]
    public async Task<IActionResult> CreateIntent([FromQuery] decimal amount, [FromQuery] string currency = "usd", CancellationToken ct = default)
    {
        var clientSecret = await _paymentService.CreatePaymentIntentAsync(amount, currency, ct);
        return Ok(new { clientSecret });
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();
        var sig = Request.Headers["Stripe-Signature"].ToString();
        var ok = await _paymentService.HandleWebhookAsync(json, sig, ct);
        return ok ? Ok() : BadRequest();
    }
}