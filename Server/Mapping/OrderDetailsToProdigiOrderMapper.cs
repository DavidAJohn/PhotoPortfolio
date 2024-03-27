using PhotoPortfolio.Shared.Models;
using PhotoPortfolio.Shared.Models.Prodigi.Common;
using PhotoPortfolio.Shared.Models.Prodigi.Orders;

namespace PhotoPortfolio.Server.Mapping;

public static class OrderDetailsToProdigiOrderMapper
{
    public static Order ToProdigiOrder(this OrderDetailsDto orderDetailsDto, string baseUri)
    {
        string merchantReference = $"PhotoPortfolio_{orderDetailsDto.Id![16..]}";
        string callbackUrl = $"{baseUri}/api/callbacks/{orderDetailsDto.Id}";

        return new Order
        {
            CallbackUrl = callbackUrl,
            MerchantReference = merchantReference,
            ShippingMethod = orderDetailsDto.ShippingMethod,
            IdempotencyKey = orderDetailsDto.Id,
            Recipient = new Recipient
            {
                Name = orderDetailsDto.Name,
                Email = orderDetailsDto.EmailAddress,
                Address = orderDetailsDto.Address
            },
            Items = orderDetailsDto.Items.Select(item => new Item
            {
                MerchantReference = item.Product.PhotoId,
                Sku = item.Product.ProdigiSku,
                Copies = item.Quantity,
                Sizing = "fillPrintArea",
                Attributes = item.Product.Options.ToAttributes(),
                RecipientCost = new Cost
                {
                    Amount = item.Total.ToString(),
                    Currency = "GBP"
                },
                Assets = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        { "printArea", "default" },
                        { "url", item.Product.ImageUri }
                    }
                }
            }).ToList(),
            Metadata = new Dictionary<string, string>
            {
                { "pi_id", orderDetailsDto.StripePaymentIntentId },
            }
        };
    }

    private static Dictionary<string, string> ToAttributes(this IEnumerable<ProductOptionSelected>? productOptions)
    {
        Dictionary<string, string>? attributes = new() { };

        if (productOptions is not null)
        {
            foreach (var attribute in productOptions)
            {
                attributes.Add(attribute.OptionLabel, attribute.OptionRef);
            }
        }

        return attributes;
    }
}
