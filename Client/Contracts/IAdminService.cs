﻿using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Client.Contracts;

public interface IAdminService
{
    Task<List<Gallery>> GetAllGalleriesAsync();
    Task<bool> UpdateGalleryAsync(Gallery gallery);
}
