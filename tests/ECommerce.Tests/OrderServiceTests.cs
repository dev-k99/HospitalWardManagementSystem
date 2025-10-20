using System;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class OrderServiceTests
{
    private ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateOrder_DecrementsStock_And_CreatesItems()
    {
        using var db = CreateDb();
        var service = new OrderService(db);
        var userId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "P1", Price = 5, Stock = 5, CategoryId = Guid.NewGuid() };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var cart = new[] { new CartItem { ProductId = product.Id, Quantity = 2 } };
        var order = await service.CreateOrderAsync(userId, cart, "pi_test");

        var saved = await db.Orders.Include(o => o.Items).FirstAsync();
        Assert.Equal(1, saved.Items.Count);
        Assert.Equal(3, (await db.Products.FindAsync(product.Id))!.Stock);
        Assert.Equal(10, saved.TotalAmount);
    }
}
