using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Entities;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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

    public async Task<bool> UpdateGalleryAsync(Gallery gallery)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");

            HttpContent galleryJson = new StringContent(JsonSerializer.Serialize(gallery));
            galleryJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PutAsync($"galleries/{gallery.Id}", galleryJson);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }

            return false;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message, ex.InnerException, ex.StatusCode);
        }
    }
}
