using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Entities;
using System.Net.Http.Json;

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
    }
}
