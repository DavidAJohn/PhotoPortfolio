using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Controllers;

[Authorize]
public class AdminController : BaseApiController
{
    private readonly IGalleryRepository _repository;

    public AdminController(IGalleryRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("galleries")]
    public async Task<List<Gallery>> GetAllGalleries()
    {
        return await _repository.GetAllAsync();
    }

    [HttpGet("galleries/{id:length(24)}")]
    public async Task<IActionResult> GetGalleryById(string id)
    {
        var gallery = await _repository.GetGalleryWithPhotos(id, true);

        if (gallery == null)
        {
            return NotFound();
        }

        return Ok(gallery);
    }

    [HttpPost("galleries")]
    public async Task<IActionResult> AddGallery(Gallery gallery)
    {
        await _repository.AddAsync(gallery);

        return CreatedAtAction(nameof(GetAllGalleries), new { id = gallery.Id }, gallery);
    }

    [HttpPut("galleries/{id:length(24)}")]
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

    [HttpDelete("galleries/{id:length(24)}")]
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
