using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;
using WardSystemProject.Models;
using System.Linq;
using System;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("WardConn");
builder.Services.AddDbContext<WardSystemDBContext>(options =>
        options.UseSqlServer(connectionString));
// Add authentication and authorization services
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<WardSystemDBContext>()
    .AddDefaultTokenProviders();
   

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Accounts/Login"; // Ensure this matches your controller
    options.AccessDeniedPath = "/Accounts/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
}).AddCookie();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed a test user
using (var scope = app.Services.CreateScope())
{
    try
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Create roles with retry logic
        string[] roles = { "Administrator", "Ward Admin", "Doctor", "Nurse", "Nursing Sister", "Script Manager", "Consumables Manager" };
        foreach (var role in roles)
        {
            try
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"Role '{role}' created successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not create role '{role}': {ex.Message}");
                // Continue with other roles
            }
        }

        // Create default administrator account if it doesn't exist
        try
        {
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                var admin = new IdentityUser
                {
                    UserName = "admin",
                    Email = "admin@wardsystem.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrator");
                    Console.WriteLine("Admin user created successfully");

                    // Create corresponding Staff record
                    try
                    {
                        var adminStaff = new Staff
                        {
                            FirstName = "System",
                            LastName = "Administrator",
                            Role = "Administrator",
                            Email = "admin@wardsystem.com",
                            IsActive = true
                        };

                        var dbContext = scope.ServiceProvider.GetRequiredService<WardSystemDBContext>();
                        dbContext.Staff.Add(adminStaff);
                        await dbContext.SaveChangesAsync();
                        Console.WriteLine("Admin staff record created successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Could not create admin staff record: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Could not create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not create admin user: {ex.Message}");
        }

        // Create default ward if it doesn't exist
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<WardSystemDBContext>();

            var generalWard = await dbContext.Wards.FirstOrDefaultAsync(w => w.Name == "General Ward");
            if (generalWard == null)
            {
                generalWard = new Ward { Name = "General Ward", IsActive = true };
                dbContext.Wards.Add(generalWard);
                await dbContext.SaveChangesAsync();
                Console.WriteLine("Default ward created successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating default ward: {ex.Message}");
            // Continue with application startup even if ward creation fails
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Database seeding failed: {ex.Message}");
        Console.WriteLine("Application will continue without initial data seeding");
        // Continue with application startup even if seeding fails 
    }
    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
    }
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    app.MapRazorPages(); // For Identity pages

    app.Run();
}
