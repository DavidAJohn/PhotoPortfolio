namespace PhotoPortfolio.Shared.Models.Prodigi.Orders;

public class Order
{
    public string Id { get; set; }
    public string Created { get; set; }
    public string CallbackUrl { get; set; }
    public string MerchantReference { get; set; }
    public string ShippingMethod { get; set; }
    public string IdempotencyKey { get; set; }
    public Status Status { get; set; }
    public List<Charge> Charges { get; set; }
    public List<Shipment> Shipments { get; set; }
    public Recipient Recipient { get; set; }
    public List<Item> Items { get; set; }
    public PackingSlip PackingSlip { get; set; }
}
