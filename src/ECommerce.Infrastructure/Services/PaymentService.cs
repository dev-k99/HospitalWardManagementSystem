using ECommerce.Core.Interfaces;
using Stripe;

namespace ECommerce.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentIntentService _paymentIntentService;

    public PaymentService()
    {
        _paymentIntentService = new PaymentIntentService();
    }

    public async Task<string> CreatePaymentIntentAsync(decimal amount, string currency, CancellationToken cancellationToken = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = currency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            }
        };
        var intent = await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);
        return intent.ClientSecret;
    }

    public Task<bool> HandleWebhookAsync(string json, string signature, CancellationToken cancellationToken = default)
    {
        // Stub for webhook verification; to be wired with endpoint secret
        return Task.FromResult(true);
    }
}