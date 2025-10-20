using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Infrastructure.Services;

public class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHostEnvironment _env;

    public RecommendationService(ApplicationDbContext dbContext, IHostEnvironment env)
    {
        _dbContext = dbContext;
        _env = env;
    }

    public async Task<IReadOnlyList<Product>> RecommendForUserAsync(Guid userId, int count = 6, CancellationToken cancellationToken = default)
    {
        // Content-based: featurize product text and recommend by cosine similarity to user's purchases
        var ml = new MLContext();

        var products = await _dbContext.Products.Include(p => p.Category).ToListAsync(cancellationToken);
        if (products.Count == 0)
        {
            return Array.Empty<Product>();
        }

        var inputs = products.Select(p => new ProductInput
        {
            Id = p.Id,
            Text = string.Join(' ', new[] { p.Name, p.Description ?? string.Empty, p.Category?.Name ?? string.Empty })
        }).ToList();

        var dataView = ml.Data.LoadFromEnumerable(inputs);
        var pipeline = ml.Transforms.Text.FeaturizeText("Features", nameof(ProductInput.Text));
        var model = pipeline.Fit(dataView);
        var transformed = model.Transform(dataView);
        var features = ml.Data.CreateEnumerable<ProductFeatures>(transformed, reuseRowObject: false).ToList();

        var userPurchases = await _dbContext.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order!.UserId == userId)
            .ToListAsync(cancellationToken);

        if (userPurchases.Count == 0)
        {
            // Fallback to popularity
            return await _dbContext.Products
                .OrderByDescending(p => p.CreatedAtUtc)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        var purchasedIds = userPurchases.Select(oi => oi.ProductId).ToHashSet();
        var purchasedVectors = features.Where(f => purchasedIds.Contains(f.Id)).Select(f => f.Features).ToList();
        if (purchasedVectors.Count == 0)
        {
            return await _dbContext.Products.Take(count).ToListAsync(cancellationToken);
        }

        // Average vector
        var dim = purchasedVectors[0].Length;
        var avg = new float[dim];
        foreach (var v in purchasedVectors)
        {
            for (int i = 0; i < dim; i++) avg[i] += v[i];
        }
        for (int i = 0; i < dim; i++) avg[i] /= purchasedVectors.Count;

        // Rank by cosine similarity
        var scored = features
            .Where(f => !purchasedIds.Contains(f.Id))
            .Select(f => new { f.Id, Score = CosineSimilarity(avg, f.Features) })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .Select(x => x.Id)
            .ToList();

        var result = products.Where(p => scored.Contains(p.Id)).ToList();
        // If fewer than count, fill with latest products
        if (result.Count < count)
        {
            var rest = products.Where(p => !result.Any(r => r.Id == p.Id) && !purchasedIds.Contains(p.Id))
                .OrderByDescending(p => p.CreatedAtUtc)
                .Take(count - result.Count)
                .ToList();
            result.AddRange(rest);
        }

        return result;
    }

    public Task TrainModelAsync(CancellationToken cancellationToken = default)
    {
        // Content-based approach is computed on the fly; no persistent model needed.
        return Task.CompletedTask;
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, normA = 0, normB = 0;
        var len = Math.Min(a.Length, b.Length);
        for (int i = 0; i < len; i++) { dot += a[i] * b[i]; normA += a[i] * a[i]; normB += b[i] * b[i]; }
        return (float)(dot / (Math.Sqrt(normA) * Math.Sqrt(normB) + 1e-8));
    }

    private sealed class ProductInput
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    private sealed class ProductFeatures
    {
        public Guid Id { get; set; }
        public float[] Features { get; set; } = Array.Empty<float>();
    }
}