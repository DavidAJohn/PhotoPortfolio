using Stripe;

namespace PhotoPortfolio.Shared.Models.Stripe;

public class LineItems
{
    public string Object { get; set; } = string.Empty;
    public StripeList<LineItem> Data { get; set; }
    public bool HasMore { get; set; } = false;
    public string Url { get; set; } = string.Empty;
}
