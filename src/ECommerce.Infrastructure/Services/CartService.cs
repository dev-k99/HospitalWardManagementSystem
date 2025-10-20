using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _dbContext;

    public CartService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CartItem>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddToCartAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId, cancellationToken);
        if (item is null)
        {
            item = new CartItem { UserId = userId, ProductId = productId, Quantity = quantity };
            _dbContext.CartItems.Add(item);
        }
        else
        {
            item.Quantity += quantity;
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId, cancellationToken);
        if (item is null) return;
        if (quantity <= 0)
        {
            _dbContext.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromCartAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId, cancellationToken);
        if (item is null) return;
        _dbContext.CartItems.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = _dbContext.CartItems.Where(ci => ci.UserId == userId);
        _dbContext.CartItems.RemoveRange(items);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}