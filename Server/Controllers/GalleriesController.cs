using Microsoft.AspNetCore.Mvc;

namespace PhotoPortfolio.Server.Controllers;

public class GalleriesController : BaseApiController
{
    private readonly IGalleryRepository _repository;

    public GalleriesController(IGalleryRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublicGalleries()
    {
        var galleries = await _repository.GetPublicGalleries();

        if (galleries == null)
        {
            return NotFound();
        }

        return Ok(galleries);
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
