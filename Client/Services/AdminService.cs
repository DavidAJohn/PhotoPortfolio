using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;
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
            var galleries = await client.GetFromJsonAsync<List<Gallery>>("admin/galleries");

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
            var gallery = await client.GetFromJsonAsync<Gallery>($"admin/galleries/{galleryId}");

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

            HttpResponseMessage response = await client.PutAsync($"admin/galleries/{gallery.Id}", galleryJson);

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

            HttpResponseMessage response = await client.PostAsync($"admin/galleries", galleryJson);

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

            HttpResponseMessage response = await client.PostAsync($"admin/photos", photoJson);

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

            HttpResponseMessage response = await client.PutAsync($"admin/photos/{photo.Id}", photoJson);

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
            var response = await client.PostAsync("admin/uploads", content);

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
            var products = await client.GetFromJsonAsync<List<Product>>("admin/products");

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
            var product = await client.GetFromJsonAsync<Product>($"admin/products/{productId}");

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

            HttpResponseMessage response = await client.PostAsync("admin/products", productJson);

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

            HttpResponseMessage response = await client.PutAsync("admin/products", productJson);

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
            var prefs = await client.GetFromJsonAsync<Preferences>("admin/preferences");

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

            HttpResponseMessage response = await client.PutAsync("admin/preferences", prefsJson);

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

    public async Task<List<OrderDetailsDto>> GetOrdersAsync(OrderSpecificationParams? orderParams)
    {
        try
        {
            var request = new HttpRequestMessage();

            if (orderParams is not null)
            {
                var queryStringParams = new Dictionary<string, string>();

                if (!string.IsNullOrWhiteSpace(orderParams.SortBy))
                {
                    queryStringParams.Add("sortBy", orderParams.SortBy);
                };

                if (!string.IsNullOrWhiteSpace(orderParams.SortOrder))
                {
                    queryStringParams.Add("sortOrder", orderParams.SortOrder);
                };

                if (!string.IsNullOrWhiteSpace(orderParams.Status))
                {
                    queryStringParams.Add("status", orderParams.Status);
                };

                if (orderParams.ExcludePaymentIncomplete)
                {
                    queryStringParams.Add("excludePaymentIncomplete", "true");
                };

                queryStringParams.Add("inLastNumberOfDays", orderParams.InLastNumberOfDays.ToString());

                request = new HttpRequestMessage(HttpMethod.Get, QueryHelpers.AddQueryString("admin/orders", queryStringParams));
            }
            else
            {
                request = new HttpRequestMessage(HttpMethod.Get, "admin/orders");
            }

            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI.Secure");
            HttpResponseMessage response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (content is not null)
                {
                    var orders = JsonSerializer.Deserialize<List<OrderDetailsDto>>(
                            content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (orders is null) return null!;

                    return orders;
                }
                else
                {
                    return null!;
                }
            }

            return null!;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message, ex.InnerException, ex.StatusCode);
        }
    }
}
