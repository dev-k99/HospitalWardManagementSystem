using ECommerce.Core.Entities;

namespace ECommerce.Core.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Guid userId, IEnumerable<CartItem> cartItems, string? stripePaymentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
}