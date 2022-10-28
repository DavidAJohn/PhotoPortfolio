using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Models.Prodigi.Quotes;
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
            var client = _httpClient.CreateClient("Prodigi.PrintAPIv4");

            var quoteJson = new StringContent(JsonSerializer.Serialize(quote));
            using HttpResponseMessage response = await client.PostAsync("quotes", quoteJson);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            var quoteResponse = JsonSerializer.Deserialize<QuoteResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (quoteResponse != null)
            {
                if (quoteResponse.Outcome.ToLower() != "created")
                {
                    Console.WriteLine("API response outcome was: " + quoteResponse.Outcome);
                    return null!;
                }

                return quoteResponse;
            }
            
            return null!;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message, ex.InnerException, ex.StatusCode);
        }
    }
}
