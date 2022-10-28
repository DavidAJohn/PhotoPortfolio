using PhotoPortfolio.Shared.Models.Prodigi.Quotes;

namespace PhotoPortfolio.Client.Contracts;

public interface IQuoteService
{
    Task<QuoteResponse> GetQuote(CreateQuoteDto quote);
}
