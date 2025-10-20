namespace ECommerce.Core.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentIntentAsync(decimal amount, string currency, CancellationToken cancellationToken = default);
    Task<bool> HandleWebhookAsync(string json, string signature, CancellationToken cancellationToken = default);
}