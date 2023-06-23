using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Models.Prodigi.Quotes;
using System.Net;
using System.Text.Json;

namespace PhotoPortfolio.Client.Services;

public class QuoteService : IQuoteService
{
    private readonly IHttpClientFactory _httpClient;
    private readonly IProductService _productService;

    public QuoteService(IHttpClientFactory httpClient, IProductService productService)
    {
        _httpClient = httpClient;
        _productService = productService;
    }

    public async Task<QuoteResponse> GetQuote(string? sku, List<BasketItem> basketItems = null!, string deliveryOption = "Standard")
    {
        if (sku is null && basketItems is null)
        {             
            return null!;
        }

        try
        {
            CreateQuoteDto quote = await CreateQuote(sku, basketItems, deliveryOption);

            var client = _httpClient.CreateClient("Prodigi.PrintAPI");

            HttpContent quoteJson = new StringContent(JsonSerializer.Serialize(quote));
            HttpResponseMessage response = await client.PostAsync("quotes", quoteJson);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var quoteResponse = JsonSerializer.Deserialize<QuoteResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (quoteResponse != null)
                {
                    Console.WriteLine("API response outcome was: " + quoteResponse.Outcome);

                    if (quoteResponse.Outcome.ToLower() != "created")
                    {
                        return null!;
                    }

                    return quoteResponse.Outcome.ToLowerInvariant() switch
                    {
                        "created" => quoteResponse,
                        "createdwithissues" => quoteResponse,
                        _ => null!,
                    };
                }
            }

            return null!;
        }
        catch
        {
            return null!;
        }
    }

    private async Task<CreateQuoteDto> CreateQuote(string? sku, List<BasketItem> basketItems = null!, string deliveryOption = "Standard")
    {
        List<CreateQuoteItemDto> items = new();
        List<Dictionary<string, string>> assetList = new();
        Dictionary<string, string> assets = new()
        {
            { "printArea", "default" }
        };
        assetList.Add(assets);

        if (basketItems is not null)
        {
            foreach (var item in basketItems)
            {
                Dictionary<string, string>? attributes = new() {};

                if (item.Product.Options is not null)
                {
                    foreach (var attribute in item.Product.Options)
                    {
                        attributes.Add(attribute.OptionLabel, attribute.OptionRef);
                    }
                }
                
                items.Add(new CreateQuoteItemDto
                {
                    Sku = item.Product.ProdigiSku,
                    Copies = item.Quantity,
                    Attributes = attributes,
                    Assets = assetList
                });
            }
        }
        else
        {
            var product = await _productService.GetProductDetailsAsync(sku);
            var attributes = product.Attributes;

            // for each item in attributes, remove all but the first array value
            Dictionary<string, string> simplifiedAttributes = new();

            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    simplifiedAttributes.Add(attribute.Key, attribute.Value[0]);
                }
            }
            
            items.Add(new CreateQuoteItemDto
            {
                Sku = sku,
                Copies = 1,
                Attributes = simplifiedAttributes,
                Assets = assetList
            });
        }

        CreateQuoteDto quote;

        if (deliveryOption == "")
        {
            quote = new()
            {
                DestinationCountryCode = "GB",
                CurrencyCode = "GBP",
                Items = items
            };
        }
        else
        {
            quote = new()
            {
                ShippingMethod = deliveryOption,
                DestinationCountryCode = "GB",
                CurrencyCode = "GBP",
                Items = items
            };
        }

        return quote;
    }
}
