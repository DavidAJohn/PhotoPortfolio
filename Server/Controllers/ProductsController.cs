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

    [HttpPost]
    public async Task<IActionResult> AddProduct(Product product)
    {
        await _repository.AddAsync(product);

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
    }
}
