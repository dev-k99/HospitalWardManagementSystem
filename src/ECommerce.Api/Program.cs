using ECommerce.Api.Extensions;
using ECommerce.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add core API services (Identity, EF, JWT, Swagger)
builder.Services.AddApiServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddResponseCompression();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ECommerce.Infrastructure.Data.ApplicationDbContext>();
        var userManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ECommerce.Core.Entities.ApplicationUser>>();
        var roleManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole<System.Guid>>>();
        await ECommerce.Infrastructure.Data.SeedData.InitializeAsync(db, userManager, roleManager);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred seeding the DB");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseSessionTracking();

app.MapControllers();

app.Run();
