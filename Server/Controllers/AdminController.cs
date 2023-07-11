using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;
using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Server.Controllers;

//[Authorize]
public class AdminController : BaseApiController
{
    private readonly IGalleryRepository _galleryRepository;
    private readonly IPhotoRepository _photoRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPreferencesRepository _preferencesRepository;
    private readonly IOrderService _orderService;
    private readonly IConfiguration _config;
    private readonly ILogger<AdminController> _logger;
    private readonly IUploadService _uploadService;

    public AdminController(IGalleryRepository galleryRepository, 
        IPhotoRepository photoRepository, 
        IProductRepository productRepository,
        IPreferencesRepository preferencesRepository,
        IOrderService orderService,
        IConfiguration config, 
        ILogger<AdminController> logger,
        IUploadService uploadService)
    {
        _galleryRepository = galleryRepository;
        _photoRepository = photoRepository;
        _productRepository = productRepository;
        _preferencesRepository = preferencesRepository;
        _orderService = orderService;
        _config = config;
        _logger = logger;
        _uploadService = uploadService;
    }

    // GALLERIES 
    //

    [HttpGet("galleries")]
    public async Task<IActionResult> GetAllGalleries()
    {
        var galleries = await _galleryRepository.GetAllAsync();

        if (galleries is null) return NotFound();

        return Ok(galleries);
    }

    [HttpGet("galleries/{id:length(24)}")]
    public async Task<IActionResult> GetGalleryById(string id)
    {
        var gallery = await _galleryRepository.GetGalleryWithPhotos(id, true);

        if (gallery == null)
        {
            return NotFound();
        }

        return Ok(gallery);
    }

    [HttpPost("galleries")]
    public async Task<IActionResult> AddGallery(Gallery gallery)
    {
        var added = await _galleryRepository.AddAsync(gallery);

        if (added is null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(GetAllGalleries), new { id = gallery.Id }, gallery);
    }

    [HttpPut("galleries/{id:length(24)}")]
    public async Task<IActionResult> UpdateGallery(string id, Gallery gallery)
    {
        var galleryToUpdate = await _galleryRepository.GetSingleAsync(x => x.Id == id);

        if (galleryToUpdate is null)
        {
            return NotFound();
        }

        gallery.Id = galleryToUpdate.Id;

        await _galleryRepository.UpdateAsync(gallery);

        return NoContent();
    }

    [HttpDelete("galleries/{id:length(24)}")]
    public async Task<IActionResult> DeleteGallery(string id)
    {
        var galleryToDelete = await _galleryRepository.GetSingleAsync(x => x.Id == id);

        if (galleryToDelete is null)
        {
            return NotFound();
        }

        await _galleryRepository.DeleteAsync(id);

        return NoContent();
    }

    // UPLOADS
    //

    [HttpPost("uploads")]
    public async Task<IActionResult> UploadFiles([FromForm] IEnumerable<IFormFile> files)
    {
        var azureConnectionString = _config["AzureUpload:AzureStorageConnectionString"];
        var azureContainerUri = _config["AzureUpload:AzureContainerUri"];
        var azureContainerName = _config["AzureUpload:AzureContainerName"];

        // check that the Azure Storage connection string is available
        if (string.IsNullOrEmpty(azureConnectionString))
        {
            _logger.LogError("Azure Storage connection string is empty or has not been set in config/env variables");
            return BadRequest("Azure Storage connection string is empty or unavailable");
        }

        // check that the Azure Storage container name is available
        if (string.IsNullOrEmpty(azureContainerName))
        {
            _logger.LogError("Azure Storage container name is empty or has not been set in config/env variables");
            return BadRequest("Azure Storage container name is empty or unavailable");
        }

        List<UploadResult> uploadResults = await _uploadService.UploadFiles(files);

        return new CreatedResult(azureContainerUri, uploadResults);
    }

    // PHOTOS
    //

    [HttpGet("photos")]
    public async Task<IActionResult> GetPhotos([FromQuery] PhotoSpecificationParams photoParams)
    {
        var emptyParams = photoParams.GetType().GetProperties().All(prop => prop.GetValue(photoParams) == null);

        var photos = new List<Photo>();

        if (emptyParams) // if all of the photoParams properties are null
        {
            photos = await _photoRepository.GetAllAsync();
        }
        else
        {
            photos = await _photoRepository.GetFilteredPhotosAsync(photoParams);
        }

        if (photos is null)
        {
            return NotFound();
        }

        return Ok(photos);
    }

    [HttpPost("photos")]
    public async Task<IActionResult> AddPhoto(Photo photo)
    {
        var added = await _photoRepository.AddAsync(photo);

        if (added is null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(GetPhotos), new { id = photo.Id }, photo);
    }

    [HttpPut("photos/{id:length(24)}")]
    public async Task<IActionResult> UpdatePhoto(string id, Photo photo)
    {
        var photoToUpdate = await _photoRepository.GetSingleAsync(x => x.Id == id);

        if (photoToUpdate is null)
        {
            return NotFound();
        }

        photo.Id = photoToUpdate.Id;

        await _photoRepository.UpdateAsync(photo);

        return NoContent();
    }

    [HttpDelete("photos/{id:length(24)}")]
    public async Task<IActionResult> DeletePhoto(string id)
    {
        var photoToDelete = await _photoRepository.GetSingleAsync(x => x.Id == id);

        if (photoToDelete is null)
        {
            return NotFound();
        }

        await _photoRepository.DeleteAsync(id);

        return NoContent();
    }

    // PRODUCTS
    //

    [HttpGet("products")]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productRepository.GetAllAsync();
        
        if (products is null)
        {
            return NotFound();
        }

        return Ok(products);
    }

    [HttpGet("products/{id:length(24)}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        var product = await _productRepository.GetSingleAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost("products")]
    public async Task<IActionResult> AddProduct(Product product)
    {
        var added = await _productRepository.AddAsync(product);

        if (added is null)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(GetAllProducts), new { id = product.Id }, product);
    }

    [HttpPut("products")]
    public async Task<IActionResult> UpdateProduct(Product product)
    {
        var productToUpdate = await _productRepository.GetSingleAsync(p => p.Id == product.Id);

        if (productToUpdate is null)
        {
            return NotFound();
        }

        await _productRepository.UpdateAsync(product);

        return NoContent();
    }

    // SITE PREFERENCES
    //

    [HttpGet("preferences")]
    public async Task<IActionResult> GetSitePreferences()
    {
        var sitePrefsId = _config["SitePreferencesId"];
        var prefs = await _preferencesRepository.GetSingleAsync(p => p.Id == sitePrefsId);

        if (prefs is null)
        {
            return NotFound();
        }

        return Ok(prefs);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdateSitePrefences(Preferences prefs)
    {
        prefs.Id = _config["SitePreferencesId"];

        var response = await _preferencesRepository.UpdateAsync(prefs);

        if (response == null)
        {
            return BadRequest();
        }

        return NoContent();
    }

    // ORDERS
    //

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] OrderSpecificationParams orderParams)
    {
        var orders = await _orderService.GetOrderDetails(orderParams);

        if (orders is not null && orders.Count > 0) return Ok(orders);

        return NotFound();
    }

}
