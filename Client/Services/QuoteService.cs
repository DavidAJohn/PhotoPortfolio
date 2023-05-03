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

    public async Task<QuoteResponse> GetQuote(CreateQuoteDto quote)
    {
        try
        {
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
}
