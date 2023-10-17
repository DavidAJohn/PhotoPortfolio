namespace PhotoPortfolio.Shared.Models.Prodigi.Orders;

public class Issue
{
    public string ObjectId { get; set; }
    public string ErrorCode { get; set; }
    public string Description { get; set; }
    public AuthorisationDetails? AuthorisationDetails { get; set; }
}
