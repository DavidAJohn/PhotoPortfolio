using MongoDB.Bson;
using PhotoPortfolio.Server.Mapping;
using PhotoPortfolio.Server.Messaging;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolio.Shared.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using PhotoPortfolioStripe = PhotoPortfolio.Shared.Models.Stripe;
using Prodigi = PhotoPortfolio.Shared.Models.Prodigi.Orders;

namespace PhotoPortfolio.Server.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPreferencesRepository _preferencesRepository;
    private readonly IConfigurationService _configService;
    private readonly IMessageSender _messageSender;
    private readonly ILogger<OrderService> _logger;
    private readonly IHttpClientFactory _httpClient;

    public OrderService(IOrderRepository orderRepository, IPreferencesRepository preferencesRepository, IConfigurationService configService, IMessageSender messageSender, ILogger<OrderService> logger, IHttpClientFactory httpClient)
    {
        _orderRepository = orderRepository;
        _preferencesRepository = preferencesRepository;
        _configService = configService;
        _messageSender = messageSender;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<string> CreatOrder(OrderBasketDto orderBasketDto)
    {
        var lineItems = orderBasketDto.BasketItems;
        var shippingMethod = orderBasketDto.ShippingMethod;
        var shippingCost = orderBasketDto.ShippingCost;

        decimal itemsCost = 0;

        foreach (var item in lineItems)
        {
            itemsCost += item.Total;
        }

        itemsCost = decimal.Round(itemsCost, 2);
        shippingCost = decimal.Round(shippingCost, 2);

        decimal totalCost = itemsCost + shippingCost;
        totalCost = decimal.Round(totalCost, 2);
        
        try
        {
            var order = new Order()
            {
                Items = lineItems,
                ShippingMethod = string.IsNullOrWhiteSpace(shippingMethod) ? "" : shippingMethod,
                OrderCreated = BsonDateTime.Create(DateTime.UtcNow),
                ItemsCost = (BsonDecimal128)itemsCost,
                ShippingCost = (BsonDecimal128)shippingCost,
                TotalCost = (BsonDecimal128)totalCost
            };

            // save to db
            var newOrder = await _orderRepository.AddAsync(order);

            if (newOrder is null) return "";

            return newOrder.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when creating Order: {message}", ex.Message);
            return "";
        }
    }

    public async Task<bool> UpdateOrder(
        string orderId,
        PhotoPortfolioStripe.Customer customer,
        PhotoPortfolioStripe.ShippingDetails shippingDetails,
        string shippingMethod,
        string paymentIntentId
        )
    {
        try
        {
            var address = new Prodigi.Address()
            {
                Line1 = shippingDetails.Address.Line1,
                Line2 = string.IsNullOrWhiteSpace(shippingDetails.Address.Line2) ? "" : shippingDetails.Address.Line2,
                PostalOrZipCode = shippingDetails.Address.PostalCode,
                CountryCode = shippingDetails.Address.Country,
                TownOrCity = shippingDetails.Address.City,
                StateOrCounty = string.IsNullOrWhiteSpace(shippingDetails.Address.State) ? "" : shippingDetails.Address.State
            };

            // get existing order details
            var existingOrder = await _orderRepository.GetSingleAsync(o => o.Id == orderId);

            if (existingOrder != null)
            {
                var order = new Order()
                {
                    Id = orderId,
                    Name = customer.Name,
                    EmailAddress = customer.EmailAddress,
                    OrderCreated = existingOrder.OrderCreated,
                    PaymentCompleted = BsonDateTime.Create(DateTime.UtcNow),
                    Items = existingOrder.Items,
                    ItemsCost = existingOrder.ItemsCost,
                    ShippingCost = existingOrder.ShippingCost,
                    TotalCost = existingOrder.TotalCost,
                    Address = address,
                    ShippingMethod = existingOrder.ShippingMethod,
                    StripePaymentIntentId = paymentIntentId,
                    Status = OrderStatus.AwaitingApproval
                };

                // update order with new details sent from Stripe
                var response = await _orderRepository.UpdateAsync(order);

                if (response is null) return false;

                return true;
            };

            _logger.LogWarning("Error when updating Order - unable to find existing order: {orderId}", orderId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when updating Order Id ({orderId}): {message}", orderId, ex.Message);
            return false;
        }
    }

    public async Task<bool> UpdateOrderCosts(OrderBasketDto orderBasketDto)
    {
        try
        {
            var existingOrder = await _orderRepository.GetSingleAsync(o => o.Id == orderBasketDto.OrderId);

            if (existingOrder != null)
            {
                decimal itemsCost = orderBasketDto.BasketItems.Sum(x => x.Total);
                decimal totalCost = itemsCost + orderBasketDto.ShippingCost;

                itemsCost = decimal.Round(itemsCost, 2);
                totalCost = decimal.Round(totalCost, 2);
                
                var order = new Order()
                {
                    Id = existingOrder.Id,
                    OrderCreated = existingOrder.OrderCreated,
                    Items = orderBasketDto.BasketItems,
                    ItemsCost = (BsonDecimal128)itemsCost,
                    ShippingCost = (BsonDecimal128)orderBasketDto.ShippingCost,
                    TotalCost = (BsonDecimal128)totalCost,
                    ShippingMethod = orderBasketDto.ShippingMethod,
                    Status = existingOrder.Status
                };

                var response = await _orderRepository.UpdateAsync(order);

                if (response is null) return false;

                return true;
            };

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when updating Order costs, Id ({orderId}): {message}", orderBasketDto.OrderId, ex.Message);
            return false;
        }
    }

    public async Task<OrderDetailsDto> GetOrderDetailsFromId(string orderId)
    {
        try
        {
            var order = await _orderRepository.GetSingleAsync(o => o.Id == orderId);

            if (order is null) return null!;

            var orderDetails = new OrderDetailsDto()
            {
                Id = order.Id,
                Name = order.Name,
                EmailAddress = order.EmailAddress,
                OrderDate = order.PaymentCompleted!.ToLocalTime(),
                Items = order.Items,
                ShippingCost = order.ShippingCost.ToDecimal(),
                TotalCost = order.TotalCost.ToDecimal(),
                Address = order.Address,
                ShippingMethod = order.ShippingMethod,
                StripePaymentIntentId = order.StripePaymentIntentId,
                Status = order.Status.ToString(),
                ProdigiDetails = order.ProdigiDetails
            };

            return orderDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting Order details, Id ({orderId}): {message}", orderId, ex.Message);
            return null!;
        }
    }

    public async Task<List<OrderDetailsDto>> GetOrderDetails(OrderSpecificationParams? orderParams)
    {
        var orders = new List<Order>();

        if (orderParams is null)
        {
            orders = await _orderRepository.GetAllAsync();
        }
        else
        {
            orders = await _orderRepository.GetFilteredOrdersAsync(orderParams);
        }

        if (orders is null) return null!;

        if (orders.Count > 0)
        {
            List<OrderDetailsDto> orderDetails = orders.ConvertAll(
                new Converter<Order, OrderDetailsDto>(OrderToDetailsConverter));

            return orderDetails;
        }

        return null!;
    }

    private OrderDetailsDto OrderToDetailsConverter(Order order)
    {
        var orderDetails = new OrderDetailsDto()
        {
            Id = order.Id,
            Name = order.Name,
            EmailAddress = order.EmailAddress,
            Items = order.Items,
            ShippingCost = order.ShippingCost.ToDecimal(),
            TotalCost = order.TotalCost.ToDecimal(),
            Address = order.Address,
            ShippingMethod = order.ShippingMethod,
            StripePaymentIntentId = order.StripePaymentIntentId,
            Status = order.Status.ToString()
        };

        if (order.PaymentCompleted is null)
        {
           orderDetails.OrderDate = order.OrderCreated.ToLocalTime();
        }
        else
        {
            orderDetails.OrderDate = order.PaymentCompleted.ToLocalTime();
        }

        if (order.ProdigiDetails is not null)
        {
            orderDetails.ProdigiDetails = order.ProdigiDetails;
        }

        return orderDetails;
    }

    public async Task<bool> ShouldApproveOrder(string orderId)
    {
        var config = _configService.GetConfiguration();
        var sitePrefsId = config.GetValue<string>("SitePreferencesId");
        if (string.IsNullOrWhiteSpace(sitePrefsId)) return false;

        var prefs = await _preferencesRepository.GetSingleAsync(p => p.Id == sitePrefsId);
        var order = await _orderRepository.GetSingleAsync(o => o.Id == orderId);

        if (prefs is null || order is null) return false;

        prefs.Metadata.TryGetValue("OrdersSentToProdigiAutomatically", out string autoApproval);
        prefs.Metadata.TryGetValue("OrderAutoApproveLimit", out string approvalLimitString);

        var totalCost = order.TotalCost.ToDecimal();
        var approvalLimit = decimal.Parse(approvalLimitString);
        bool approveDecision = false;

        approveDecision = autoApproval switch
        {
            "AutoApproveAll" => true,
            "ManuallyApproveAll" => false,
            _ => false
        };

        // check for auto-approval below a limit
        if (autoApproval == "AutoApproveBelow")
        {
            if (totalCost < approvalLimit) approveDecision = true;
            else approveDecision = false;
        }

        return approveDecision;
    }

    public async Task<bool> ApproveOrder(string orderId)
    {
        var order = await _orderRepository.GetSingleAsync(o => o.Id == orderId);
        if (order is null) return false;

        order.Status = OrderStatus.Approved;
        var updateResponse = await _orderRepository.UpdateAsync(order);

        if (updateResponse is null) return false;

        OrderDetailsDto orderDetails = OrderToDetailsConverter(order);

        // send message to Azure Service Bus queue
        var messageSent = await _messageSender.SendMessage(orderDetails.ToOrderApprovedMessage());

        if (!messageSent) return false;

        return true;
    }

    public async Task<bool> CreateProdigiOrder(OrderDetailsDto order)
    {
        try
        {
            var prodigiOrder = order.ToProdigiOrder();

            var config = _configService.GetConfiguration();
            var prodigiApiKey = config["Prodigi:ApiKey"];
            var prodigiApiUri = config["Prodigi:ApiUri"];

            var client = _httpClient.CreateClient();

            JsonSerializerOptions serializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            HttpContent orderJson = new StringContent(JsonSerializer.Serialize(prodigiOrder, serializerOptions));
            orderJson.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            orderJson.Headers.Add("X-API-Key", prodigiApiKey);
            HttpResponseMessage response = await client.PostAsync(prodigiApiUri + "orders", orderJson);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var orderResponse = JsonSerializer.Deserialize<Prodigi.OrderResponse>(
                                       responseContent,
                                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (orderResponse != null)
                {
                    if (orderResponse.Outcome.ToLower() != "created")
                    {
                        _logger.LogWarning("Prodigi warning -> Print API response outcome was: {response}", orderResponse.Outcome);
                    }

                    await UpdateOrderWithProdigiDetails(orderResponse);

                    return orderResponse.Outcome.ToLower() switch
                    {
                        "created" => true,
                        "createdwithissues" => true,
                        "alreadyexists" => true,
                        _ => false,
                    };
                }

                _logger.LogWarning("Prodigi warning -> Print API response was 200 OK, but content was null");
                return false;
            }

            if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
            {
                var problemDetailsJson = await response.Content.ReadAsStringAsync();
                _logger.LogError("Prodigi error -> Print API response - status code: {statusCode}, details: {problemDetails}", response.StatusCode, problemDetailsJson);
                return false;
            }
            else
            {
                _logger.LogError("Prodigi error -> Print API response - status code: {statusCode}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating Prodigi Order: {message}", ex.Message);
            return false;
        }
    }

    private async Task UpdateOrderWithProdigiDetails(Prodigi.OrderResponse orderResponse)
    {
        if (orderResponse.Outcome.ToLower() == "alreadyexists")
        {
            _logger.LogWarning("Prodigi warning -> Print API response indicates a duplicate order creation request. Idempotency key: {key}", orderResponse.Order!.IdempotencyKey);
        }
        else 
        {
            try
            {
                var order = await _orderRepository.GetSingleAsync(o => o.Id == orderResponse.Order!.IdempotencyKey);

                if (order is not null)
                {
                    order.ProdigiDetails = orderResponse;
                    order.Status = OrderStatus.InProgress;
                    var updateResponse = await _orderRepository.UpdateAsync(order);

                    if (updateResponse is null)
                    {
                        _logger.LogError("Error updating Order with Prodigi details, Id : {orderId}", order.Id);
                    }
                    else
                    {
                        _logger.LogInformation("Order updated with Prodigi details, Id : {orderId}", order.Id);
                    }
                }
                else
                {
                    _logger.LogInformation("Error updating Order with Prodigi details. Could not find Id : {orderId}", orderResponse.Order!.IdempotencyKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Order with Prodigi details: {message}", ex.Message);
            }
        }
    }
}
