using PhotoPortfolio.Shared.Models;

public class BasketItem
{
    public int Quantity { get; set; }
    public PhotoProduct Product { get; set; }

    public decimal Total { get; set; }
}
