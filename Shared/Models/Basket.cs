namespace PhotoPortfolio.Shared.Models;

public class Basket
{
    public List<BasketItem> BasketItems { get; set; } = new();

    public decimal BasketTotal
    {
        get
        {
            decimal total = (decimal)0.0;

            foreach (var item in BasketItems)
            {
                total += item.Total;
            }

            return total;
        }
    }
    public DateTime LastAccessed { get; set; }
    public int TimeToLiveInSeconds { get; set; } = 1200; // 20 mins
}
