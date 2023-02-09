using PhotoPortfolio.Shared.Models.Prodigi.Quotes;

namespace PhotoPortfolio.Server.Contracts;

public interface IQuoteService
{
    Task<QuoteResponse> GetQuote(CreateQuoteDto quote);
}
