using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace PhotoPortfolio.Tests.Integration.Infrastructure;

public class StripeWebhookHelperMethods
{
    // Parts of the code below have been adapted from the official open-source Stripe .NET library.
    // Specifically, the method of generating the Stripe-Signature header value is taken from here:
    // https://github.com/stripe/stripe-dotnet/blob/master/src/Stripe.net/Services/Events/EventUtility.cs#L193

    private static readonly UTF8Encoding SafeUTF8 
        = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public static Stripe.Event CreateCheckoutSessionCompleteEvent(string orderId)
    {
        var webhookEvent = new Stripe.Event
        {
            Id = "evt_" + Guid.NewGuid().ToString(),
            Object = "event",
            ApiVersion = "2022-11-15",
            Created = DateTime.UtcNow,
            Account = "acct_1",
            Livemode = false,
            Type = "checkout.session.completed",
            PendingWebhooks = 0,
            Request = new Stripe.EventRequest
            {
                Id = "req_" + Guid.NewGuid().ToString(),
                IdempotencyKey = Guid.NewGuid().ToString()
            },
            Data = new Stripe.EventData
            {
                Object = new Stripe.Checkout.Session
                {
                    Id = $"cs_{Guid.NewGuid()}",
                    Object = "checkout.session",
                    AfterExpiration = null,
                    AmountSubtotal = 10000,
                    AmountTotal = 10525,
                    AutomaticTax = new Stripe.Checkout.SessionAutomaticTax
                    {
                        Enabled = false,
                        Status = null
                    },
                    BillingAddressCollection = null,
                    CancelUrl = "https://localhost/checkout/cancel",
                    ClientReferenceId = null,
                    Currency = "gbp",
                    PaymentStatus = "paid",
                    CustomerDetails = new Stripe.Checkout.SessionCustomerDetails
                    {
                        Name = "Test User",
                        Email = "test@test.com"
                    },
                    ShippingDetails = new Stripe.Checkout.SessionShippingDetails
                    {
                        Address = new Stripe.Address
                        {
                            Line1 = "Test Address",
                            Line2 = "Test Address 2",
                            City = "Test City",
                            Country = "Test Country",
                            PostalCode = "Test Postcode",
                            State = "Test State"
                        },
                        Carrier = "Test Carrier",
                        Name = "Test Name",
                        Phone = "Test Phone",
                        TrackingNumber = "Test Tracking Number"
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "order_id", orderId },
                        { "shipping_method", "Standard" }
                    },
                    PaymentIntentId = $"pi_{Guid.NewGuid()}"
                }
            }
        };

        return webhookEvent;
    }

    public static Stripe.Event CreatePaymentIntentCreatedEvent()
    {
        var webhookEvent = new Stripe.Event
        {
            Id = "evt_" + Guid.NewGuid().ToString(),
            Object = "event",
            ApiVersion = "2022-11-15",
            Created = DateTime.UtcNow,
            Account = "acct_1",
            Livemode = false,
            Type = "payment_intent.created",
            PendingWebhooks = 0,
            Request = new Stripe.EventRequest
            {
                Id = "req_" + Guid.NewGuid().ToString(),
                IdempotencyKey = Guid.NewGuid().ToString()
            },
            Data = new Stripe.EventData
            {
                Object = new Stripe.PaymentIntent
                {
                    Id = $"pi_{Guid.NewGuid()}",
                    Object = "payment_intent"
                }
            }
        };

        return webhookEvent;
    }

    public static Stripe.Event CreatePaymentIntentSucceededEvent()
    {
        var webhookEvent = new Stripe.Event
        {
            Id = "evt_" + Guid.NewGuid().ToString(),
            Object = "event",
            ApiVersion = "2022-11-15",
            Created = DateTime.UtcNow,
            Account = "acct_1",
            Livemode = false,
            Type = "payment_intent.succeeded",
            PendingWebhooks = 0,
            Request = new Stripe.EventRequest
            {
                Id = "req_" + Guid.NewGuid().ToString(),
                IdempotencyKey = Guid.NewGuid().ToString()
            },
            Data = new Stripe.EventData
            {
                Object = new Stripe.PaymentIntent
                {
                    Id = $"pi_{Guid.NewGuid()}",
                    Object = "payment_intent"
                }
            }
        };

        return webhookEvent;
    }

    public static Stripe.Event CreateChargeSucceededEvent()
    {
        var webhookEvent = new Stripe.Event
        {
            Id = "evt_" + Guid.NewGuid().ToString(),
            Object = "event",
            ApiVersion = "2022-11-15",
            Created = DateTime.UtcNow,
            Account = "acct_1",
            Livemode = false,
            Type = "charge.succeeded",
            PendingWebhooks = 0,
            Request = new Stripe.EventRequest
            {
                Id = "req_" + Guid.NewGuid().ToString(),
                IdempotencyKey = Guid.NewGuid().ToString()
            },
            Data = new Stripe.EventData
            {
                Object = new Stripe.Charge
                {
                    Id = $"ch_{Guid.NewGuid()}",
                    Object = "charge",
                    Status = "succeeded"
                }
            }
        };

        return webhookEvent;
    }

    public static Stripe.Event CreateChargeFailedEvent()
    {
        var webhookEvent = new Stripe.Event
        {
            Id = "evt_" + Guid.NewGuid().ToString(),
            Object = "event",
            ApiVersion = "2022-11-15",
            Created = DateTime.UtcNow,
            Account = "acct_1",
            Livemode = false,
            Type = "charge.failed",
            PendingWebhooks = 0,
            Request = new Stripe.EventRequest
            {
                Id = "req_" + Guid.NewGuid().ToString(),
                IdempotencyKey = Guid.NewGuid().ToString()
            },
            Data = new Stripe.EventData
            {
                Object = new Stripe.Charge
                {
                    Id = $"ch_{Guid.NewGuid()}",
                    Object = "charge",
                    Status = "failed"
                }
            }
        };

        return webhookEvent;
    }

    public static HttpContent CreateWebhookContent(Stripe.Event webhookEvent, string stripeWebhookSecret)
    {
        // For details of how the Stripe webhook signature is created, see https://stripe.com/docs/webhooks#verify-manually
        // This is used here to create a valid webhook event for testing purposes

        var datetimeOffset = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var jsonPayloadIndented = JsonConvert.SerializeObject(webhookEvent, Formatting.Indented); // don't switch to using System.Text.Json here, it doesn't work with the Stripe.net library
        var jsonPayload = jsonPayloadIndented.Replace("\r", string.Empty); // remove additional '\r' carriage returns that Newtonsoft.Json adds
        var signedPayload = $"{datetimeOffset}.{jsonPayload}";

        var secretBytes = SafeUTF8.GetBytes(stripeWebhookSecret);
        var payloadBytes = SafeUTF8.GetBytes(signedPayload);

        var signature = string.Empty;

        using (var hmac = new HMACSHA256(secretBytes))
        {
            var hash = hmac.ComputeHash(payloadBytes);
            signature = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        HttpContent webhookContent = new StringContent(jsonPayload);
        webhookContent.Headers.Add("Stripe-Signature", $"t={datetimeOffset},v1={signature}");
        webhookContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return webhookContent;
    }
}
