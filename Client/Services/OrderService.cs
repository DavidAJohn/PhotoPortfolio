using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
        catch
        {
            return null!;
        }
    }

    public async Task<string> CreateCheckoutSession(OrderBasketDto orderBasketDto, bool userIsAdmin)
    {
        try
        {
            var client = _httpClient.CreateClient(userIsAdmin ? "PhotoPortfolio.ServerAPI.Secure" : "PhotoPortfolio.ServerAPI");

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
        catch
        {
            return null!;
        }
    }

    public async Task<CheckoutSessionResponse> GetOrderFromCheckoutSession(string sessionId)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI");
            HttpResponseMessage response = await client.GetAsync($"payments/session/{sessionId}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var checkoutResponse = await response.Content.ReadFromJsonAsync<CheckoutSessionResponse>();

                if (checkoutResponse is null) return null!;

                return checkoutResponse;
            }

            return null!;
        }
        catch
        {
            return null!;
        }
    }

    public async Task<OrderDetailsDto> GetOrderDetailsFromId(string orderId)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI");
            var order = await client.GetFromJsonAsync<OrderDetailsDto>($"orders/{orderId}");

            if (order is null) return null!;

            return order;
        }
        catch
        {
            return null!;
        }
    }

    public async Task<bool> ShouldApproveOrder(string orderId)
    {
        try
        {
            var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI");
            HttpResponseMessage response = await client.GetAsync($"orders/approve/{orderId}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var approve = await response.Content.ReadAsStringAsync();

                // OK response should only return true, but as insurance:
                if (string.IsNullOrEmpty(approve) || approve == "false") return false;

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
