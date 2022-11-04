using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Entities;
using System.Net.Http.Json;

namespace PhotoPortfolio.Client.Services;

public class AdminService : IAdminService
{
    private readonly IHttpClientFactory _httpClient;

    public AdminService(IHttpClientFactory httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Gallery>> GetAllGalleriesAsync()
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");
            var galleries = await client.GetFromJsonAsync<List<Gallery>>("galleries");

            if (galleries is null) return null!;

            return galleries;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message, ex.InnerException, ex.StatusCode);
        }
    }
}
