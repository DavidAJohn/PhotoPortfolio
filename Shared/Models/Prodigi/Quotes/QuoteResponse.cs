namespace PhotoPortfolio.Shared.Models.Prodigi.Quotes;

public class QuoteResponse : BaseResponse
{
    public List<Issue> Issues { get; set; } = null!;
    public List<Quote> Quotes { get; set; } = null!;
}
