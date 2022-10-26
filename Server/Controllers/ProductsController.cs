using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Controllers;

public class ProductsController : BaseApiController
{
    private readonly IProductRepository _repository;

    public ProductsController(IProductRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<List<Product>> GetProducts()
    {
        return await _repository.GetAllAsync();
    }

    [HttpGet("{photoId:length(24)}")]
    public async Task<List<Product>> GetProductsForPhoto(string photoId)
    {
        var products = await _repository.GetProductsForPhotoAsync(photoId);

        return products;
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(Product product)
    {
        await _repository.AddAsync(product);

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
    }
}
