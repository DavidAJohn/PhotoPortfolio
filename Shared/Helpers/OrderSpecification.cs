using LinqKit;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;
using System.Linq.Expressions;

namespace PhotoPortfolio.Shared.Helpers;

public class OrderSpecification
{
    public static Expression<Func<Order, bool>> CreateOrderSpecificationPredicate(OrderSpecificationParams orderParams)
    {
        // as a fallback, this initially creates a predicate that would return all records,
        // which we still want to do if the orderParams properties are null
        Expression<Func<Order, bool>> predicate = PredicateBuilder.New<Order>(_ => true);

        if (orderParams != null)
        {
            if (orderParams.Status is not null)
            {
                // converting o.Status ToString() then using Equals when building the predicate does not work
                // as expected, so the orderParams.Status string value is converted to an OrderStatus instead
                string[] statusNames = Enum.GetNames<OrderStatus>();
                var statusAsOrderStatus = OrderStatus.PaymentIncomplete;

                foreach (string name in statusNames)
                {
                    if (name == orderParams.Status)
                    {
                        if (Enum.TryParse(orderParams.Status, out OrderStatus enumStatus))
                        {
                            statusAsOrderStatus = enumStatus;
                        }
                    }
                }

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
