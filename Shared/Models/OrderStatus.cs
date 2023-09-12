namespace PhotoPortfolio.Shared.Models;

public enum OrderStatus
{
    PaymentIncomplete,
    AwaitingApproval,
    Approved,
    InProgress,
    Completed,
    Cancelled
}
