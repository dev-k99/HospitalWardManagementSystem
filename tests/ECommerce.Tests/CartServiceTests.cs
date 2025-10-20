using System;
using System.Threading.Tasks;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class CartServiceTests
{
    private ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddToCart_AddsOrIncrements()
    {
        using var db = CreateDb();
        var service = new CartService(db);
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        db.Products.Add(new Product { Id = productId, Name = "Test", Price = 10, Stock = 10, CategoryId = Guid.NewGuid()});
        await db.SaveChangesAsync();

        await service.AddToCartAsync(userId, productId, 1);
        await service.AddToCartAsync(userId, productId, 2);

        var items = await service.GetCartAsync(userId);
        Assert.Single(items);
        Assert.Equal(3, items[0].Quantity);
    }
}
