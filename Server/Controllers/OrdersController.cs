using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Server.Controllers;

public class OrdersController : BaseApiController
{
    private readonly ILogger<AdminController> _logger;
    private readonly IOrderService _orderService;

    public OrdersController(ILogger<AdminController> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderBasketDto orderBasketDto)
    {
        var orderId = await _orderService.CreatOrder(orderBasketDto.BasketItems, orderBasketDto.ShippingMethod);

        if (!string.IsNullOrWhiteSpace(orderId))
        {
            _logger.LogInformation("Order added to database successfully: {Id}", orderId);
            return Ok(orderId);
        }
        else
        {
            _logger.LogInformation("Order could NOT be added to database: {Id}", orderId);
            return BadRequest();
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCheckoutSessionFromId(string id)
    {
        var order = await _orderService.GetOrderDetailsFromId(id);

        if (order == null) return NotFound();

        return Ok(order);
    }
}
