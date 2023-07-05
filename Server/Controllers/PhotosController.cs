using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Entities;
using PhotoPortfolio.Shared.Helpers;

namespace PhotoPortfolio.Server.Controllers;

public class PhotosController : BaseApiController
{
    private readonly IPhotoRepository _repository;

    public PhotosController(IPhotoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetPhotos([FromQuery] PhotoSpecificationParams photoParams)
    {
        var emptyParams = photoParams.GetType().GetProperties().All(prop => prop.GetValue(photoParams) == null);

        var photos = new List<Photo>();

        if (emptyParams) // if all of the photoParams properties are null
        {
            photos = await _repository.GetAllAsync();
        }
        else
        {
            photos = await _repository.GetFilteredPhotosAsync(photoParams);
        }

        if (photos is null)
        {
            return NotFound();
        }

        return Ok(photos);
    }

    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> GetPhotoById(string id)
    {
        var photo = await _repository.GetSingleAsync(x => x.Id == id);

        if (photo == null)
        {
            return NotFound();
        }

        return Ok(photo);
    }
}
