﻿using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Client.Contracts;

public interface IAdminService
{
    Task<List<Gallery>> GetAllGalleriesAsync();
    Task<Gallery> GetGalleryByIdAsync(string galleryId);
    Task<bool> UpdateGalleryAsync(Gallery gallery);
    Task<bool> CreateGalleryAsync(CreateGalleryDto gallery);
    Task<bool> AddPhotoAsync(Photo photo);
    Task<List<UploadResult>> UploadPhotos(MultipartFormDataContent content);
    Task<List<Product>> GetProductsAsync();
}
