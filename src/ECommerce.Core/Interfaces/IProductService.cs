using ECommerce.Core.Entities;
using ECommerce.Core.Models;

namespace ECommerce.Core.Interfaces;

public interface IProductService
{
    Task<PagedResult<Product>> GetProductsAsync(ProductQuery query, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}