namespace PhotoPortfolio.Shared.Models;

public class OrderBasketDto
{
    public List<BasketItem> BasketItems { get; set; }
    public string ShippingMethod { get; set; }
    public string? OrderId { get; set; } = string.Empty;
}
