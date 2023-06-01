using PhotoPortfolio.Shared.Models.Prodigi.Quotes;

namespace PhotoPortfolio.Client.Contracts;

public interface IQuoteService
{
    Task<QuoteResponse> GetQuote(string sku, List<BasketItem> basketItems = null!, string deliveryOption = "Standard");
}
