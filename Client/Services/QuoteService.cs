using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Models.Prodigi.Quotes;
using System.Net;
using System.Text.Json;

namespace PhotoPortfolio.Client.Services;

public class QuoteService : IQuoteService
{
    private readonly IHttpClientFactory _httpClient;

    public QuoteService(IHttpClientFactory httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<QuoteResponse> GetQuote(string? sku, List<BasketItem> basketItems = null!, string deliveryOption = "Standard")
    {
        if (sku is null && basketItems is null)
        {             
            return null!;
        }

        try
        {
            CreateQuoteDto quote = CreateQuote(sku, basketItems, deliveryOption);

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

    private static CreateQuoteDto CreateQuote(string? sku, List<BasketItem> basketItems = null!, string deliveryOption = "Standard")
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
                items.Add(new CreateQuoteItemDto
                {
                    Sku = item.Product.ProdigiSku,
                    Copies = item.Quantity,
                    Assets = assetList
                });
            }
        }
        else
        {
            items.Add(new CreateQuoteItemDto
            {
                Sku = sku,
                Copies = 1,
                Assets = assetList
            });
        }

        CreateQuoteDto quote = new()
        {
            ShippingMethod = deliveryOption,
            DestinationCountryCode = "GB",
            CurrencyCode = "GBP",
            Items = items
        };

        return quote;
    }
}
