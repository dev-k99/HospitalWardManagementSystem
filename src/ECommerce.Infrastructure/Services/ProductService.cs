using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _dbContext;

    public ProductService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<Product>> GetProductsAsync(ProductQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> products = _dbContext.Products.AsNoTracking().Include(p => p.Category);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.ToLower();
            products = products.Where(p => p.Name.ToLower().Contains(term) || (p.Description != null && p.Description.ToLower().Contains(term)));
        }
        if (query.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == query.CategoryId);
        }
        if (query.MinPrice.HasValue)
        {
            products = products.Where(p => p.Price >= query.MinPrice.Value);
        }
        if (query.MaxPrice.HasValue)
        {
            products = products.Where(p => p.Price <= query.MaxPrice.Value);
        }

        var total = await products.CountAsync(cancellationToken);
        var items = await products
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Products.AsNoTracking().Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Products.FindAsync(new object?[] { id }, cancellationToken);
        if (entity != null)
        {
            _dbContext.Products.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public Task<IEnumerable<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<Category>>(_dbContext.Categories.AsNoTracking().AsEnumerable());

    public async Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Categories.FindAsync(new object?[] { categoryId }, cancellationToken);
        if (entity != null)
        {
            _dbContext.Categories.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}