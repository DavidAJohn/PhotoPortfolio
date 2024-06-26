﻿using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Contracts;

public interface IGalleryRepository : IBaseRepository<Gallery>
{
    Task<Gallery> GetGalleryWithPhotos(string id, bool includePrivate = false);
    Task<List<Gallery>> GetPublicGalleries();
}
