namespace PhotoPortfolio.Shared.Models.Prodigi.Orders;

public class Recipient
{
    public string Name { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Address Address { get; set; }
}
