using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace PhotoPortfolio.Server.Controllers;

public class PaymentsController : BaseApiController
{
    private readonly ILogger<AdminController> _logger;
    private readonly IPaymentService _paymentService;

    public PaymentsController(ILogger<AdminController> logger, IPaymentService paymentService)
    {
        _logger = logger;
        _paymentService = paymentService;
    }

    [HttpPost("session")]
    public async Task<IActionResult> CreateCheckoutSession(List<BasketItem> basketItems)
    {
        // TO DO: create a quote service (similar to client) to check the price of each basket item

        // then supply it to the payment service
        var session = await _paymentService.CreateCheckoutSession(basketItems);
        var url = session.Url;

        return Ok(url);
    }
}
