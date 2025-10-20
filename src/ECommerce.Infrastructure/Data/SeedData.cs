using System.Security.Claims;
using ECommerce.Core.Entities;
using ECommerce.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext db, UserManager<ApplicationUser> users, RoleManager<IdentityRole<Guid>> roles)
    {
        await db.Database.MigrateAsync();

        if (!await roles.RoleExistsAsync(Roles.Admin))
        {
            await roles.CreateAsync(new IdentityRole<Guid>(Roles.Admin));
        }
        if (!await roles.RoleExistsAsync(Roles.User))
        {
            await roles.CreateAsync(new IdentityRole<Guid>(Roles.User));
        }

        if (await users.FindByEmailAsync("admin@shop.local") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@shop.local",
                Email = "admin@shop.local"
            };
            await users.CreateAsync(admin, "Admin123!");
            await users.AddToRoleAsync(admin, Roles.Admin);
        }

        if (!db.Categories.Any())
        {
            var cat1 = new Category { Name = "Electronics" };
            var cat2 = new Category { Name = "Clothing" };
            var cat3 = new Category { Name = "Home" };
            db.Categories.AddRange(cat1, cat2, cat3);
            await db.SaveChangesAsync();

            var rnd = new Random(42);
            var categories = await db.Categories.ToListAsync();
            var products = new List<Product>();
            for (int i = 1; i <= 50; i++)
            {
                var category = categories[rnd.Next(categories.Count)];
                products.Add(new Product
                {
                    Name = $"Product {i}",
                    Description = "Sample product description",
                    Price = Math.Round((decimal)(rnd.NextDouble() * 100 + 10), 2),
                    ImageUrl = "https://placehold.co/600x400",
                    CategoryId = category.Id,
                    Stock = rnd.Next(1, 100)
                });
            }
            db.Products.AddRange(products);
            await db.SaveChangesAsync();
        }
    }
}