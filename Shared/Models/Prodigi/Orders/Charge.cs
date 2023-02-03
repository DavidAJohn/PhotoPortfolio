using PhotoPortfolio.Shared.Models.Prodigi.Common;

namespace PhotoPortfolio.Shared.Models.Prodigi.Orders;

public class Charge
{
    public string Id { get; set; }
    public string ProdigiInvoiceNumber { get; set; }
    public Cost TotalCost { get; set; }
    public List<ChargeItem> ChargeItems { get; set; }
}