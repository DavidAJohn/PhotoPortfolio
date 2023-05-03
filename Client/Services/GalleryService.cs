using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Entities;
using System.Net.Http.Json;

namespace PhotoPortfolio.Client.Services;

public class GalleryService : IGalleryService
{
    private readonly IHttpClientFactory _httpClient;

    public GalleryService(IHttpClientFactory httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Gallery>> GetGalleriesAsync()
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI");
            var galleries = await client.GetFromJsonAsync<List<Gallery>>("galleries");

            if (galleries is null) return null!;

            return galleries;
        }
        catch
        {
            return null!;
        }
    }

    public async Task<Gallery> GetGalleryByIdAsync(string id)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI");
            var gallery = await client.GetFromJsonAsync<Gallery>($"galleries/{id}");

            if (gallery is null) return null!;

            return gallery;
        }
        catch
        {
            return null!;
        }
    }
}
