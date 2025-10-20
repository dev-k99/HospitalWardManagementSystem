using System.Diagnostics;
using ECommerce.Infrastructure.Data;
using ECommerce.Core.Entities;
using System.Security.Claims;

namespace ECommerce.Api.Middleware;

public class SessionTrackingMiddleware
{
    private readonly RequestDelegate _next;

    public SessionTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        try
        {
            Guid? userId = null;
            var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(sub, out var uid))
            {
                userId = uid;
            }
            db.SessionEvents.Add(new SessionEvent
            {
                UserId = userId,
                PageVisited = context.Request.Path,
                DurationSeconds = (int)Math.Round(sw.Elapsed.TotalSeconds)
            });
            await db.SaveChangesAsync();
        }
        catch
        {
            // swallow analytics failures
        }
    }
}

public static class SessionTrackingExtensions
{
    public static IApplicationBuilder UseSessionTracking(this IApplicationBuilder app)
        => app.UseMiddleware<SessionTrackingMiddleware>();
}
