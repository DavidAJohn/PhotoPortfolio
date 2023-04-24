namespace PhotoPortfolio.Shared.Models;

public class OrderBasketDto
{
    public List<BasketItem> BasketItems { get; set; }
    public decimal ShippingCost { get; set; } = 0m;
    public string ShippingMethod { get; set; }
    public string? OrderId { get; set; } = string.Empty;
}
