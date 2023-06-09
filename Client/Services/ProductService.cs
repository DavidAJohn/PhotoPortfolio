using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models.Prodigi.Products;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PhotoPortfolio.Client.Services
{
    public class ProductService : IProductService
    {
        private readonly IHttpClientFactory _httpClient;

        public ProductService(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Product>> GetProductsForPhotoAsync(string photoId)
        {
            try
            {
                var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI");
                var products = await client.GetFromJsonAsync<List<Product>>($"products/{photoId}");

                if (products is null) return null!;

                return products;
            }
            catch 
            { 
                return null!; 
            }
        }

        public async Task<ProductDetails> GetProductDetailsAsync(string sku)
        {
            try
            {
                var client = _httpClient.CreateClient("Prodigi.PrintAPI");
                var response = await client.GetAsync($"products/{sku}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var productResponse = JsonSerializer.Deserialize<ProductDetailsResponse>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    Console.WriteLine("API response outcome was: " + productResponse.Outcome);

                    if (productResponse.Outcome.ToLower() != "ok")
                    {
                        return null!;
                    }

                    return productResponse.Product;
                }

                return null!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
    }
}
