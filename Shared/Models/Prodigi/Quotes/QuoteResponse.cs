namespace PhotoPortfolio.Shared.Models.Prodigi.Quotes;

public class QuoteResponse : BaseResponse
{
    public List<Quote> Quotes { get; set; } = null!;
}
