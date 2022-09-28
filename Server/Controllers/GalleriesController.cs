using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Controllers;

public class GalleriesController : BaseApiController
{
    private readonly IGalleryRepository _repository;

    public GalleriesController(IGalleryRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<List<Gallery>> GetPublicGalleries()
    {
        return await _repository.GetPublicGalleries();
    }

    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> GetGalleryById(string id)
    {
        var gallery = await _repository.GetGalleryWithPhotos(id);

        if (gallery == null)
        {
            return NotFound();
        }

        return Ok(gallery);
    }
}
