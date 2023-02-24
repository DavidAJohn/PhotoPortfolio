namespace PhotoPortfolio.Shared.Models;

public class OrderBasketDto
{
    public List<BasketItem> BasketItems { get; set; }
    public string ShippingMethod { get; set; }
}
