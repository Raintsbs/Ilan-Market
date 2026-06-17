using AdvertisementApp.Business.Interface;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace AdvertisementApp.Business.Service
{
    public class PaymentWebhookService : IPaymentWebhookService
    {
        private const int MaxRetries = 5;

        private readonly AdvertisementAppDbContext _db;
        private readonly IPaymentService _payments;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentWebhookService> _logger;

        public PaymentWebhookService(
            AdvertisementAppDbContext db,
            IPaymentService payments,
            IConfiguration config,
            ILogger<PaymentWebhookService> logger)
        {
            _db = db;
            _payments = payments;
            _config = config;
            _logger = logger;
        }

        public async Task<bool> ProcessStripeWebhookAsync(string json, string? signature)
        {
            await RetryFailedEventsAsync();

            var webhookSecret = _config["Stripe:WebhookSecret"];
            if (string.IsNullOrWhiteSpace(webhookSecret))
                return true;

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature invalid");
                return false;
            }

            return await ProcessEventAsync(
                "stripe",
                stripeEvent.Id,
                stripeEvent.Type,
                json,
                async () =>
                {
                    if (stripeEvent.Type != "checkout.session.completed")
                        return true;

                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    if (session?.Id == null) return false;
                    return await _payments.CompleteStripeSessionAsync(session.Id);
                });
        }

        public async Task<bool> ProcessIyzicoCallbackAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            await RetryFailedEventsAsync();
            var eventId = $"iyzico:{token}";
            return await ProcessEventAsync(
                "iyzico",
                eventId,
                "checkout.completed",
                null,
                () => _payments.CompleteIyzicoTokenAsync(token));
        }

        public async Task RetryFailedEventsAsync(int maxBatch = 10)
        {
            var failed = await _db.PaymentWebhookEvents
                .Where(e => e.Status == "failed" && e.RetryCount < MaxRetries)
                .OrderBy(e => e.CreatedAt)
                .Take(maxBatch)
                .ToListAsync();

            foreach (var evt in failed)
            {
                var ok = await RetrySingleAsync(evt);
                if (!ok && evt.RetryCount >= MaxRetries)
                {
                    evt.Status = "dead";
                    await _db.SaveChangesAsync();
                }
            }
        }

        private async Task<bool> ProcessEventAsync(
            string provider,
            string eventId,
            string eventType,
            string? payloadJson,
            Func<Task<bool>> handler)
        {
            var existing = await _db.PaymentWebhookEvents
                .FirstOrDefaultAsync(e => e.Provider == provider && e.EventId == eventId);
            if (existing?.Status == "processed")
                return true;

            var row = existing ?? new PaymentWebhookEvent
            {
                Provider = provider,
                EventId = eventId,
                EventType = eventType,
                PayloadJson = payloadJson,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
            };
            if (existing == null)
                _db.PaymentWebhookEvents.Add(row);

            try
            {
                var ok = await handler();
                if (ok)
                {
                    row.Status = "processed";
                    row.ProcessedAt = DateTime.UtcNow;
                    row.LastError = null;
                }
                else
                {
                    row.Status = "failed";
                    row.RetryCount++;
                    row.LastError = "Handler returned false";
                }
            }
            catch (Exception ex)
            {
                row.Status = "failed";
                row.RetryCount++;
                row.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                _logger.LogError(ex, "Webhook processing failed {Provider} {EventId}", provider, eventId);
            }

            await _db.SaveChangesAsync();
            return row.Status == "processed";
        }

        private async Task<bool> RetrySingleAsync(PaymentWebhookEvent evt)
        {
            try
            {
                bool ok = evt.Provider switch
                {
                    "stripe" when evt.EventType == "checkout.session.completed" =>
                        await RetryStripeSessionAsync(evt.PayloadJson),
                    "iyzico" when evt.EventId.StartsWith("iyzico:") =>
                        await _payments.CompleteIyzicoTokenAsync(evt.EventId["iyzico:".Length..]),
                    _ => true,
                };

                if (ok)
                {
                    evt.Status = "processed";
                    evt.ProcessedAt = DateTime.UtcNow;
                    evt.LastError = null;
                }
                else
                {
                    evt.RetryCount++;
                    evt.LastError = "Retry handler returned false";
                    if (evt.RetryCount >= MaxRetries) evt.Status = "dead";
                }
            }
            catch (Exception ex)
            {
                evt.RetryCount++;
                evt.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                if (evt.RetryCount >= MaxRetries) evt.Status = "dead";
            }

            await _db.SaveChangesAsync();
            return evt.Status == "processed";
        }

        private async Task<bool> RetryStripeSessionAsync(string? payloadJson)
        {
            if (string.IsNullOrWhiteSpace(payloadJson)) return false;
            var stripeEvent = EventUtility.ParseEvent(payloadJson);
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            if (session?.Id == null) return false;
            return await _payments.CompleteStripeSessionAsync(session.Id);
        }
    }
}
