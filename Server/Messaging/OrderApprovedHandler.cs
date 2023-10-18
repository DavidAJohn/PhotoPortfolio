using MediatR;
using PhotoPortfolio.Server.Mapping;

namespace PhotoPortfolio.Server.Messaging;

public class OrderApprovedHandler : IRequestHandler<OrderApproved>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderApprovedHandler> _logger;

    public OrderApprovedHandler(IOrderService orderService, ILogger<OrderApprovedHandler> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    async Task IRequestHandler<OrderApproved>.Handle(OrderApproved request, CancellationToken cancellationToken)
    {
        var orderProcessed = await _orderService.CreateProdigiOrder(request.ToOrderDetails());

        if (!orderProcessed)
        {
            _logger.LogError("Error creating order with Prodigi. Order Id: {orderId}", request.Id);
            throw new Exception("Error creating order with Prodigi.");
        }

        _logger.LogInformation("Order response from Prodigi successfully processed. Order Id: {orderId}", request.Id);

        return;
    }
}
