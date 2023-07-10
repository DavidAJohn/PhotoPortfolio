using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Server.Contracts;

public interface IQuoteService
{
    Task<OrderBasketDto> GetBasketQuote(OrderBasketDto orderBasketDto, bool userIsAdmin);
}
