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
    public async Task<List<Gallery>> GetGalleries()
    {
        return await _repository.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGalleryById(string id)
    {
        var gallery = await _repository.GetGalleryWithPhotos(id);

        if (gallery == null)
        {
            return NotFound();
        }

        return Ok(gallery);
    }

    [HttpPost]
    public async Task<IActionResult> AddGallery(Gallery gallery)
    {
        await _repository.AddAsync(gallery);

        return CreatedAtAction(nameof(GetGalleries), new { id = gallery.Id }, gallery);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> UpdateGallery(string id, Gallery gallery)
    {
        var galleryToUpdate = await _repository.GetSingleAsync(x => x.Id == id);

        if (galleryToUpdate is null)
        {
            return NotFound();
        }

        gallery.Id = galleryToUpdate.Id;

        await _repository.UpdateAsync(gallery);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> DeleteGallery(string id)
    {
        var galleryToDelete = await _repository.GetSingleAsync(x => x.Id == id);

        if (galleryToDelete is null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);

        return NoContent();
    }
}
