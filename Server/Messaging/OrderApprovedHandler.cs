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
        var orderCreated = await _orderService.CreateProdigiOrder(request.ToOrderDetailsMessage());

        if (!orderCreated)
        {
            _logger.LogError("Error when creating order in Prodigi");
            throw new Exception("Error when creating order in Prodigi");
        }

        _logger.LogInformation("Order sent to Prodigi");

        return;
    }
}
