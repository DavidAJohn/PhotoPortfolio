﻿using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Client.Contracts;

public interface IAdminService
{
    Task<List<Gallery>> GetAllGalleriesAsync();
    Task<Gallery> GetGalleryByIdAsync(string galleryId);
    Task<bool> UpdateGalleryAsync(Gallery gallery);
    Task<bool> CreateGalleryAsync(CreateGalleryDto gallery);
    Task<bool> AddPhotoAsync(Photo photo);
    Task<bool> UpdatePhotoAsync(Photo photo);
    Task<List<UploadResult>> UploadPhotos(MultipartFormDataContent content);
    Task<List<Product>> GetProductsAsync();
    Task<Product> GetProductByIdAsync(string productId);
    Task<Product> AddProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<Preferences> GetSitePreferencesAsync();
    Task<bool> UpdateSitePreferencesAsync(Preferences prefs);
    Task<List<OrderDetailsDto>> GetOrdersAsync(OrderSpecificationParams? orderParams);
    Task<bool> ApproveOrderAsync(OrderDetailsDto order);
}
