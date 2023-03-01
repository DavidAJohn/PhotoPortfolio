using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;
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

    public async Task<Gallery> GetGalleryByIdAsync(string galleryId)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");
            var gallery = await client.GetFromJsonAsync<Gallery>($"galleries/{galleryId}");

            if (gallery is null) return null!;

            return gallery;
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
            gallery.LastUpdated = DateTime.UtcNow;

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

    public async Task<bool> CreateGalleryAsync(CreateGalleryDto gallery)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");

            HttpContent galleryJson = new StringContent(JsonSerializer.Serialize(gallery));
            galleryJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PostAsync($"galleries", galleryJson);

            if (response.StatusCode == HttpStatusCode.Created)
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

    public async Task<bool> AddPhotoAsync(Photo photo)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");

            HttpContent photoJson = new StringContent(JsonSerializer.Serialize(photo));
            photoJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PostAsync($"photos", photoJson);

            if (response.StatusCode == HttpStatusCode.Created)
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

    public async Task<bool> UpdatePhotoAsync(Photo photo)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");

            HttpContent photoJson = new StringContent(JsonSerializer.Serialize(photo));
            photoJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PutAsync($"photos/{photo.Id}", photoJson);

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

    public async Task<List<UploadResult>> UploadPhotos(MultipartFormDataContent content)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");
            var response = await client.PostAsync("uploads", content);

            var newUploadResults = await response.Content.ReadFromJsonAsync<List<UploadResult>>();

            if (newUploadResults is not null)
            {
                return newUploadResults;
            }

            return null!;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message, ex.InnerException, ex.StatusCode);
        }
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");
            var products = await client.GetFromJsonAsync<List<Product>>("products");

            if (products is null) return null!;

            return products;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message, ex.InnerException, ex.StatusCode);
        }
    }

    public async Task<Product> GetProductByIdAsync(string productId)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");
            var product = await client.GetFromJsonAsync<Product>($"products/{productId}");

            if (product is null) return null!;

            return product;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message, ex.InnerException, ex.StatusCode);
        }
    }

    public async Task<Product> AddProductAsync(Product product)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");

            HttpContent productJson = new StringContent(JsonSerializer.Serialize(product));
            productJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PostAsync("products", productJson);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                var returnedProduct = await response.Content.ReadFromJsonAsync<Product>();

                if (returnedProduct is null) return null!;

                return returnedProduct;
            }

            return null!;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message, ex.InnerException, ex.StatusCode);
        }
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");

            HttpContent productJson = new StringContent(JsonSerializer.Serialize(product));
            productJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PutAsync("products", productJson);

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

    public async Task<Preferences> GetSitePreferencesAsync()
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");
            var prefs = await client.GetFromJsonAsync<Preferences>("preferences");

            if (prefs is null) return null!;

            return prefs;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message, ex.InnerException, ex.StatusCode);
        }
    }

    public async Task<bool> UpdateSitePreferencesAsync(Preferences prefs)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");

            HttpContent prefsJson = new StringContent(JsonSerializer.Serialize(prefs));
            prefsJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PutAsync("preferences", prefsJson);

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
