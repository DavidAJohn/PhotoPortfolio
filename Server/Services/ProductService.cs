using PhotoPortfolio.Shared.Models.Prodigi.Products;

namespace PhotoPortfolio.Server.Services;

public class ProductService : IProductService
{
    private readonly IHttpClientFactory _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IHttpClientFactory httpClient, IConfiguration config, ILogger<ProductService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<ProductDetails> GetProductDetails(string sku)
    {
        try
        {
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Add("X-API-Key", _config["Prodigi:ApiKey"]);
            var apiUrl = _config["Prodigi:ApiUri"];
            var productResponse = await client.GetFromJsonAsync<ProductDetailsResponse>($"{apiUrl}products/{sku}");

            if (productResponse != null)
            {
                Console.WriteLine("API response outcome was: " + productResponse.Outcome);

                if (productResponse.Outcome.ToLower() != "ok")
                {
                    _logger.LogWarning("Prodigi warning -> Print API response outcome was: {response}", productResponse.Outcome);
                    return null!;
                }

                return productResponse.Product;
            }

            return null!;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when requesting product details from Prodigi Print API: {message}", ex.Message);
            return null!;
        }
    }
}
