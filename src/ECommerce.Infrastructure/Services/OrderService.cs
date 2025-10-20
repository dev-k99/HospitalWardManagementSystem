using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _dbContext;

    public OrderService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order> CreateOrderAsync(Guid userId, IEnumerable<CartItem> cartItems, string? stripePaymentId, CancellationToken cancellationToken = default)
    {
        var items = cartItems.ToList();
        var productIds = items.Select(i => i.ProductId).ToList();
        var products = await _dbContext.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(cancellationToken);
        decimal total = 0m;
        foreach (var item in items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            if (product.Stock < item.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
            }
            product.Stock -= item.Quantity;
            total += product.Price * item.Quantity;
        }

        var order = new Order
        {
            UserId = userId,
            StripePaymentId = stripePaymentId,
            Status = "Paid",
            TotalAmount = total,
            Items = items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = products.First(p => p.Id == i.ProductId).Price
            }).ToList()
        };

        using var trx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _dbContext.CartItems.RemoveRange(_dbContext.CartItems.Where(ci => ci.UserId == userId));
        await _dbContext.SaveChangesAsync(cancellationToken);
        await trx.CommitAsync(cancellationToken);
        return order;
    }

    public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders.Include(o => o.Items).ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders.Include(o => o.Items).ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}