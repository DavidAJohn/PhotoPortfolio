using PhotoPortfolio.Shared.Models.Prodigi.Common;

namespace PhotoPortfolio.Shared.Models.Prodigi.Orders;

public class AuthorisationDetails
{
    public string AuthorisationUrl { get; set; }
    public Cost PaymentDetails { get; set; }
}
