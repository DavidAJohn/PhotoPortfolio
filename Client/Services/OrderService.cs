using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PhotoPortfolio.Client.Services;

public class OrderService : IOrderService
{
    private readonly IHttpClientFactory _httpClient;

    public OrderService(IHttpClientFactory httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> CreateInitialOrder(OrderBasketDto orderBasketDto)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI");

            HttpContent basketJson = new StringContent(JsonSerializer.Serialize(orderBasketDto));
            basketJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync("orders", basketJson);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var orderId = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(orderId)) return null!;

                return orderId;
            }

            return null!;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message);
        }
    }

    public async Task<string> CreateCheckoutSession(OrderBasketDto orderBasketDto)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI");

            HttpContent basketJson = new StringContent(JsonSerializer.Serialize(orderBasketDto));
            basketJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync("payments/session", basketJson);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var checkoutUrl = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(checkoutUrl)) return null!;

                return checkoutUrl;
            }

            return null!;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(ex.Message);
        }
    }
}
