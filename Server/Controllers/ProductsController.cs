using Microsoft.AspNetCore.Mvc;

namespace PhotoPortfolio.Server.Controllers;

public class ProductsController : BaseApiController
{
    private readonly IProductRepository _repository;

    public ProductsController(IProductRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _repository.GetAllAsync();
        
        if (products == null || products.Count == 0) return NotFound();

        return Ok(products);
    }
}
