using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Server.Controllers;

public class OrdersController : BaseApiController
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IOrderService _orderService;

    public OrdersController(ILogger<OrdersController> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderBasketDto orderBasketDto)
    {
        var orderId = await _orderService.CreatOrder(orderBasketDto);

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
    public async Task<IActionResult> GetOrderDetailsFromId(string id)
    {
        var order = await _orderService.GetOrderDetailsFromId(id);

        if (order == null) return NotFound();

        return Ok(order);
    }

    [HttpGet("approve/{id}")]
    public async Task<IActionResult> ShouldApproveOrderFromId(string id)
    {
        var approve = await _orderService.ShouldApproveOrder(id);

        if (!approve) return BadRequest();

        return Ok(approve);
    }
}
