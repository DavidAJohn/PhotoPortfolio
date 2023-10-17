namespace PhotoPortfolio.Shared.Models.Prodigi.Orders;

public class Status
{
    public string Stage { get; set; }
    public Details Details { get; set; }
    public List<Issue>? Issues { get; set; }
}
