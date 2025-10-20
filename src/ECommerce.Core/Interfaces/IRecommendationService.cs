using ECommerce.Core.Entities;

namespace ECommerce.Core.Interfaces;

public interface IRecommendationService
{
    Task<IReadOnlyList<Product>> RecommendForUserAsync(Guid userId, int count = 6, CancellationToken cancellationToken = default);
    Task TrainModelAsync(CancellationToken cancellationToken = default);
}