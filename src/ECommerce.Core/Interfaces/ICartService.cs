using ECommerce.Core.Entities;

namespace ECommerce.Core.Interfaces;

public interface ICartService
{
    Task<IReadOnlyList<CartItem>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddToCartAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task UpdateQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task RemoveFromCartAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
}