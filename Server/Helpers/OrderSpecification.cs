using LinqKit;
using PhotoPortfolio.Shared.Entities;
using System.Linq.Expressions;

namespace PhotoPortfolio.Server.Helpers;

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
                predicate = predicate.And(o => o.Status.Equals(orderParams.Status));
            }

            if (orderParams.CustomerEmail is not null)
            {
                predicate = predicate.And(o => o.EmailAddress.Equals(orderParams.CustomerEmail));
            }

            var numOfDays = orderParams.InLastNumberOfDays * -1;
            predicate = predicate.And(o => o.OrderCreated.ToUniversalTime() > DateTime.Now.AddDays(numOfDays));
        }

        return predicate;
    }
}
