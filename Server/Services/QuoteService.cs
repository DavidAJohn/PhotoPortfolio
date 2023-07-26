using PhotoPortfolio.Shared.Models;
using PhotoPortfolio.Shared.Models.Prodigi.Quotes;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PhotoPortfolio.Server.Services;

public class QuoteService : IQuoteService
{
    private readonly IHttpClientFactory _httpClient;
    private readonly IConfigurationService _configService;
    private readonly ILogger<QuoteService> _logger;
    private readonly IPhotoRepository _photoRepository;

    public QuoteService(IHttpClientFactory httpClient, IConfigurationService configService, ILogger<QuoteService> logger, IPhotoRepository photoRepository)
    {
        _httpClient = httpClient;
        _configService = configService;
        _logger = logger;
        _photoRepository = photoRepository;
    }

    public async Task<OrderBasketDto> GetBasketQuote(OrderBasketDto orderBasketDto, bool userIsAdmin)
    {
        List<CreateQuoteItemDto> items = new();
        List<Dictionary<string, string>> assetList = new();
        Dictionary<string, string> assets = new()
        {
            { "printArea", "default" }
        };
        assetList.Add(assets);

        foreach (BasketItem item in orderBasketDto.BasketItems)
        {
            Dictionary<string, string>? attributes = new() { };

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
                Copies = 1,
                Attributes = attributes,
                Assets = assetList
            });
        }

        CreateQuoteDto quote = new CreateQuoteDto()
        {
            ShippingMethod = orderBasketDto.ShippingMethod,
            DestinationCountryCode = "GB",
            CurrencyCode = "GBP",
            Items = items
        };

        var quoteResponse = await GetProdigiQuote(quote);

        if (quoteResponse is not null)
        {
            _logger.LogInformation("Prodigi --> Quote received");

            var issues = quoteResponse.Issues;

            if (issues is not null && issues.Count > 0)
            {
                foreach (var issue in issues)
                {
                    _logger.LogWarning("Prodigi quote response: {response} -> Issue: Error Code: {code}, Description: {description} ",
                        quoteResponse.Outcome, issue.ErrorCode, issue.Description);
                }
            }
            
            var quotes = quoteResponse.Quotes;
            var quoteReturned = quotes.FirstOrDefault();

            if (quoteReturned is not null && quoteReturned.CostSummary is not null)
            {
                decimal shippingCost = 0m;

                if (!string.IsNullOrWhiteSpace(quoteReturned.CostSummary.Shipping!.Amount))
                {
                    shippingCost = decimal.Parse(quoteReturned.CostSummary.Shipping.Amount);
                }

                orderBasketDto.ShippingCost = shippingCost;

                // also confirm the basket item costs are still correct
                var quoteItems = quoteReturned.Items;

                foreach (BasketItem item in orderBasketDto.BasketItems)
                {
                    var unitCost = decimal.Parse(quoteItems.FirstOrDefault(i => i.Sku == item.Product.ProdigiSku).UnitCost.Amount);
                    var taxUnitCost = decimal.Parse(quoteItems.FirstOrDefault(i => i.Sku == item.Product.ProdigiSku).TaxUnitCost.Amount);

                    var productId = item.Product.Id;
                    var photoId = item.Product.PhotoId;
                    var photo = await _photoRepository.GetSingleAsync(p => p.Id == photoId);

                    if (photo is not null)
                    {
                        if (photo.Products is not null)
                        {
                            var product = photo.Products.FirstOrDefault(p => p.Id == productId) ?? null!;

                            if (product is not null)
                            {
                                var markupPercentage = GetMarkupPercentage(product, userIsAdmin);
                                decimal markupMultiplier = ((decimal)markupPercentage / 100) + 1;
                                var quoteItemTotal = (unitCost + taxUnitCost) * markupMultiplier;

                                if (quoteItemTotal != item.Total)
                                {
                                    _logger.LogWarning("Basket item price differed from quoted price. OrderId: {orderId} -> Basket Price: {basket} - Quoted Price: {quote}", orderBasketDto.OrderId, item.Total, quoteItemTotal);
                                }

                                item.Total = quoteItemTotal;
                            }
                        }
                    }
                }
            }

            return orderBasketDto;
        }

        _logger.LogWarning("Prodigi --> Quote NOT received");
        return null!;
    }

    private async Task<QuoteResponse> GetProdigiQuote(CreateQuoteDto quote)
    {
        try
        {
            var config = _configService.GetConfiguration();
            var prodigiApiKey = config["Prodigi:ApiKey"];
            var prodigiApiUri = config["Prodigi:ApiUri"];

            var client = _httpClient.CreateClient();

            JsonSerializerOptions serializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            HttpContent quoteJson = new StringContent(JsonSerializer.Serialize(quote, serializerOptions));
            quoteJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            quoteJson.Headers.Add("X-API-Key", prodigiApiKey);
            HttpResponseMessage response = await client.PostAsync(prodigiApiUri + "quotes", quoteJson);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var quoteResponse = JsonSerializer.Deserialize<QuoteResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (quoteResponse != null)
                {
                    if (quoteResponse.Outcome.ToLower() != "created")
                    {
                        _logger.LogWarning("Prodigi warning -> Print API response outcome was: {response}", quoteResponse.Outcome);
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
        catch (Exception ex)
        {
            _logger.LogError("Error when requesting a quote from Prodigi Print API: {message}", ex.Message);
            return null!;
        }
    }

    private static int GetMarkupPercentage(PhotoProduct product, bool userIsAdmin)
    {
        if (!userIsAdmin)
        {
            return product.MarkupPercentage;
        }
        else
        {
            return 0;
        }
    }
}
