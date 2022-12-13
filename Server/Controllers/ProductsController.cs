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
}
