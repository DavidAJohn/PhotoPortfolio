using LinqKit;
using PhotoPortfolio.Shared.Entities;
using System.Linq.Expressions;

namespace PhotoPortfolio.Shared.Helpers;

public class OrderSpecification
{
    public static Expression<Func<Order, bool>> CreateOrderSpecificationPredicate(OrderSpecificationParams orderParams)
    {
        // as a fallback, this initially creates a predicate that would return all records,
        // which we still want to do if the orderParams properties (checked below) are null
        Expression<Func<Order, bool>> predicate = PredicateBuilder.New<Order>(_ => true);

        if (orderParams != null) // now we potentially add conditions which would filter the records
        {
            if (orderParams.Status is not null)
            {
                // converting o.Status ToString() then using Equals when building the predicate does not work,
                // so converted orderParams.Status string value to an OrderStatus instead
                var statusAsOrderStatus = orderParams.Status switch
                {
                    "NotReady" => OrderStatus.NotReady,
                    "AwaitingApproval" => OrderStatus.AwaitingApproval,
                    "InProgress" => OrderStatus.InProgress,
                    "Completed" => OrderStatus.Completed,
                    "Cancelled" => OrderStatus.Cancelled
                };

                predicate = predicate.And(o => o.Status.Equals(statusAsOrderStatus));
            }
            
            if (orderParams.CustomerEmail is not null)
            {
                predicate = predicate.And(o => o.EmailAddress.Contains(orderParams.CustomerEmail));
            }

            if (orderParams.InLastNumberOfDays <= 0) // checks for any zero or negative values inadvertently supplied
            {
                orderParams.InLastNumberOfDays = 365;
            }

            var numOfDays = orderParams.InLastNumberOfDays * -1;
            predicate = predicate.And(o => o.OrderCreated > DateTime.Now.AddDays(numOfDays));
        }

        return predicate;
    }
}
