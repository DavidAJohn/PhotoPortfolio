using PhotoPortfolio.Shared.Models;

public class BasketItem
{
    public int Quantity { get; set; }
    public ProductBasketItemDto Product { get; set; }

    public decimal Total { get; set; }
    public string ShippingMethod { get; set; } = "Standard";
}
