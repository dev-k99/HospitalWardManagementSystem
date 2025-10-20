using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/admin/metrics")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    public record SalesPoint(string date, decimal total);
    public record TopProduct(string name, int quantity);
    public record CountDto(int count);
    public record PageHit(string path, int hits);

    [HttpGet("sales")] // /api/admin/metrics/sales?days=30
    public async Task<ActionResult<IEnumerable<SalesPoint>>> Sales([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.Date.AddDays(-Math.Abs(days));
        var data = await _db.Orders
            .Where(o => o.CreatedAtUtc >= since)
            .GroupBy(o => o.CreatedAtUtc.Date)
            .Select(g => new SalesPoint(g.Key.ToString("yyyy-MM-dd"), g.Sum(x => x.TotalAmount)))
            .OrderBy(sp => sp.date)
            .ToListAsync(ct);
        return Ok(data);
    }

    [HttpGet("top-products")] // /api/admin/metrics/top-products?limit=10
    public async Task<ActionResult<IEnumerable<TopProduct>>> TopProducts([FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var data = await _db.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Qty)
            .Take(limit)
            .Join(_db.Products, x => x.ProductId, p => p.Id, (x, p) => new TopProduct(p.Name, x.Qty))
            .ToListAsync(ct);
        return Ok(data);
    }

    [HttpGet("active-users")] // /api/admin/metrics/active-users?windowMinutes=10
    public async Task<ActionResult<CountDto>> ActiveUsers([FromQuery] int windowMinutes = 10, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddMinutes(-Math.Abs(windowMinutes));
        var count = await _db.SessionEvents
            .Where(s => s.TimestampUtc >= since && s.UserId != null)
            .Select(s => s.UserId!)
            .Distinct()
            .CountAsync(ct);
        return Ok(new CountDto(count));
    }

    [HttpGet("top-pages")] // /api/admin/metrics/top-pages?days=7&limit=10
    public async Task<ActionResult<IEnumerable<PageHit>>> TopPages([FromQuery] int days = 7, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-Math.Abs(days));
        var data = await _db.SessionEvents
            .Where(s => s.TimestampUtc >= since && s.PageVisited != null)
            .GroupBy(s => s.PageVisited!)
            .Select(g => new PageHit(g.Key, g.Count()))
            .OrderByDescending(x => x.hits)
            .Take(limit)
            .ToListAsync(ct);
        return Ok(data);
    }
}