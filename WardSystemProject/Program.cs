using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Core.Audit;
using WardSystemProject.Core.Interfaces;
using WardSystemProject.Data;
using WardSystemProject.Features.PatientCare;
using WardSystemProject.Features.PatientManagement;
using WardSystemProject.Models;
using WardSystemProject.Validators;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure ────────────────────────────────────────────────────────────
// IHttpContextAccessor is needed by AuditInterceptor to resolve the current user.
builder.Services.AddHttpContextAccessor();

// Register the audit interceptor as a singleton — interceptors are instantiated
// once and reused; IHttpContextAccessor is safe as a singleton dependency.
builder.Services.AddSingleton<AuditInterceptor>();

// ── Database ──────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("WardConn");
builder.Services.AddDbContext<WardSystemDBContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Lock account after 5 consecutive failed attempts for 15 minutes.
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers      = true;
})
    .AddEntityFrameworkStores<WardSystemDBContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Accounts/Login";
    options.AccessDeniedPath = "/Accounts/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

// ── Authorization — named policies (no raw role strings scattered in views) ───
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageWard",         p => p.RequireRole("Administrator"));
    options.AddPolicy("CanAdmitPatients",       p => p.RequireRole("Ward Admin"));
    options.AddPolicy("CanPrescribe",           p => p.RequireRole("Doctor"));
    options.AddPolicy("CanAdministerMeds",      p => p.RequireRole("Nurse", "Nursing Sister"));
    options.AddPolicy("CanDispenseHighSchedule",p => p.RequireRole("Nursing Sister"));
    options.AddPolicy("CanProcessScripts",      p => p.RequireRole("Script Manager"));
    options.AddPolicy("CanManageStock",         p => p.RequireRole("Consumables Manager"));
});

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IVitalSignService, VitalSignService>();
builder.Services.AddScoped<PatientFolderPdfService>();

// ── FluentValidation ──────────────────────────────────────────────────────────
// Validators are registered from the Validators assembly.
// FluentValidation integrates with ModelState so controllers work unchanged.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<AdmitPatientValidator>();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Auto-migrate + Seed ───────────────────────────────────────────────────────
// Applies any pending EF Core migrations on startup (safe to run repeatedly).
// Scoped block closes before app.Run() — scope is not held open for app lifetime.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WardSystemDBContext>();
    db.Database.Migrate();
    await SeedDatabaseAsync(scope.ServiceProvider, app.Configuration);
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS: tell browsers to always use HTTPS for this host.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Accounts}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

// ── Seed helper ───────────────────────────────────────────────────────────────
static async Task SeedDatabaseAsync(IServiceProvider services, IConfiguration config)
{
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var dbContext   = services.GetRequiredService<WardSystemDBContext>();
    var logger      = services.GetRequiredService<ILogger<Program>>();

    // Create all application roles
    string[] roles =
    {
        "Administrator", "Ward Admin", "Doctor",
        "Nurse", "Nursing Sister", "Script Manager", "Consumables Manager"
    };

    foreach (var role in roles)
    {
        try
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role '{Role}' created.", role);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not create role '{Role}'.", role);
        }
    }

    // Create default administrator — credentials come from config, NOT hardcoded.
    // In development: appsettings.Development.json → SeedAdmin section.
    // In production:  environment variables or Azure Key Vault.
    var adminUsername = config["SeedAdmin:Username"] ?? "admin";
    var adminEmail    = config["SeedAdmin:Email"]    ?? "admin@wardsystem.com";
    var adminPassword = config["SeedAdmin:Password"];

    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        logger.LogWarning("SeedAdmin:Password not configured — skipping admin user creation.");
        return;
    }

    try
    {
        var adminUser = await userManager.FindByNameAsync(adminUsername);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName       = adminUsername,
                Email          = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
                logger.LogInformation("Default admin user created.");

                // Create matching Staff record — store IdentityUserId for fast lookup
                var adminStaff = new Staff
                {
                    FirstName      = "System",
                    LastName       = "Administrator",
                    Role           = "Administrator",
                    Email          = adminEmail,
                    IdentityUserId = adminUser.Id,
                    IsActive       = true
                };
                dbContext.Staff.Add(adminStaff);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Admin Staff record created.");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Could not create admin user: {Errors}", errors);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Admin user seeding failed.");
    }

    // Create default ward if absent
    try
    {
        if (!await dbContext.Wards.AnyAsync(w => w.Name == "General Ward"))
        {
            dbContext.Wards.Add(new Ward { Name = "General Ward", IsActive = true });
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Default 'General Ward' created.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not create default ward.");
    }
}
