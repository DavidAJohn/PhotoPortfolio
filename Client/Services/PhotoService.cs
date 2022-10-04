using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Shared.Entities;
using System.Net.Http.Json;

namespace PhotoPortfolio.Client.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IHttpClientFactory _httpClient;

        public PhotoService(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Photo> GetPhotoByIdAsync(string id)
        {
            try
            {
                var client = _httpClient.CreateClient("PhotoPortfolio.ServerAPI");
                var photo = await client.GetFromJsonAsync<Photo>($"photos/{id}");

                if (photo is null) return null!;

                return photo;
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException(ex.Message);
            }
        }
    }
}
