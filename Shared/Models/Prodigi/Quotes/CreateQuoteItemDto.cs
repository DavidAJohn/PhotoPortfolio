namespace PhotoPortfolio.Shared.Models.Prodigi.Quotes;

public class CreateQuoteItemDto
{
    public string Sku { get; set; } = string.Empty;
    public int Copies { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    public List<Dictionary<string, string>>? Assets { get; set; }
}
