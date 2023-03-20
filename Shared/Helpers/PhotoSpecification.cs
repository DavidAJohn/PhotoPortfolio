using LinqKit;
using PhotoPortfolio.Shared.Entities;
using System.Linq.Expressions;

namespace PhotoPortfolio.Shared.Helpers;

public class PhotoSpecification
{
    public static Expression<Func<Photo, bool>> CreatePhotoSpecificationPredicate(PhotoSpecificationParams photoParams)
    {
        // as a fallback, this initially creates a predicate that would return all records,
        // which we still want to do if the photoParams properties (checked below) are null
        Expression<Func<Photo, bool>> predicate = PredicateBuilder.New<Photo>(_ => true);

        if (photoParams != null) // now we potentially add conditions which would filter the records
        {
            if (photoParams.GalleryId is not null)
            {
                predicate = predicate.And(p => p.GalleryId.Equals(photoParams.GalleryId));
            }

            if (photoParams.Title is not null)
            {
                predicate = predicate.Or(p => p.Title.Contains(photoParams.Title));
            }

        }

        return predicate;
    }
}
