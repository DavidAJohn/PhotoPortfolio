using Microsoft.AspNetCore.Mvc;
using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Server.Controllers;

public class PhotosController : BaseApiController
{
    private readonly IPhotoRepository _repository;

    public PhotosController(IPhotoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<List<Photo>> GetPhotos([FromQuery] PhotoSpecificationParams photoParams)
    {
        var emptyParams = photoParams.GetType().GetProperties().All(prop => prop.GetValue(photoParams) == null);

        if (emptyParams) // if all of the photoParams properties are null
        {
            return await _repository.GetAllAsync();
        }

        var photos = await _repository.GetFilteredPhotosAsync(photoParams);

        return photos;
    }

    [HttpGet("{id:length(24)}")]
    public async Task<Photo> GetPhotoById(string id)
    {
        return await _repository.GetSingleAsync(x => x.Id == id);
    }
}
