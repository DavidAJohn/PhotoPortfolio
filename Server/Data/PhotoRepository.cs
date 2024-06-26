﻿using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;
using System.Linq.Expressions;

namespace PhotoPortfolio.Server.Data;

public class PhotoRepository : BaseRepository<Photo>, IPhotoRepository
{
    private readonly MongoContext _context;
    private readonly static string _collectionName = "photos";

    public PhotoRepository(MongoContext context) : base(context, _collectionName)
    {
        _context = context;
    }

    public Expression<Func<Photo, object>> SortByPredicate { get; set; } = null!;

    public async Task<List<Photo>> GetFilteredPhotosAsync(PhotoSpecificationParams photoParams)
    {
        var _photos = _context.Database.GetCollection<Photo>(_collectionName);
        var photoPredicate = PhotoSpecification.CreatePhotoSpecificationPredicate(photoParams);

        var filter = Builders<Photo>.Filter.Where(photoPredicate);

        if (photoParams.SortBy is not null)
        {
            SortByPredicate = photoParams.SortBy.ToLower() switch
            {
                "dateadded" => p => p.DateAdded,
                "title" => p => p.Title,
                "filename" => p => p.FileName,
                "datetaken" => p => p.Metadata.DateTaken,
                "custom" => p => p.GallerySortOrder,
                _ => p => p.DateAdded
            };
        }

        if (photoParams.SortOrder is not null)
        {
            if (photoParams.SortOrder.ToLower() == "desc")
            {
                return await _photos
                    .Find(filter)
                    .SortByDescending(SortByPredicate)
                    .ToListAsync();
            }

            return await _photos
                .Find(filter)
                .SortBy(SortByPredicate)
                .ToListAsync();
        }

        return await _photos
            .Find(filter)
            .SortBy(p => p.DateAdded)
            .ToListAsync();
    }
}
