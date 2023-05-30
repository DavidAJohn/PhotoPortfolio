using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Models;
using System.Net.Http.Json;

namespace PhotoPortfolio.Client.Services;

public class CreditService : ICreditService
{
    private readonly IHttpClientFactory _httpClient;
    private readonly ILogger<CreditService> _logger;

    public CreditService(IHttpClientFactory httpClient, ILogger<CreditService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<PhotoCredit>> GetPhotoCredits()
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI");
            var credits = await client.GetFromJsonAsync<List<PhotoCredit>>($"credits");

            if (credits is null) return null!;

            return credits;
        }
        catch (Exception ex)
        {
            _logger.LogError("Unable to retrieve photo credits: {message}", ex.Message);
            return null!;
        }
    }
}
