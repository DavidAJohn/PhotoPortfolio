using PhotoPortfolio.Shared.Models.Prodigi.Quotes;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PhotoPortfolio.Server.Services;

public class QuoteService : IQuoteService
{
    private readonly IHttpClientFactory _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<QuoteService> _logger;

    public QuoteService(IHttpClientFactory httpClient, IConfiguration config, ILogger<QuoteService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<QuoteResponse> GetQuote(CreateQuoteDto quote)
    {
        try
        {
            var client = _httpClient.CreateClient();

            HttpContent quoteJson = new StringContent(JsonSerializer.Serialize(quote));
            quoteJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            quoteJson.Headers.Add("X-API-Key", _config["Prodigi:ApiKey"]);
            HttpResponseMessage response = await client.PostAsync(_config["Prodigi:ApiUri"] + "quotes", quoteJson);

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
        catch (Exception ex)
        {
            _logger.LogError("Error when requesting a quote from Prodigi Print API: {message}", ex.Message);
            return null!;
        }
    }
}
