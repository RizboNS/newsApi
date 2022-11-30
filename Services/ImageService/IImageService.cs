using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.ImageService
{
    public interface IImageService
    {
        Task<ServiceResponse<ImageSavedDto>> SaveImage(string imageAsBase64, string imageFileType, Guid storyId, string storyCategory);
    }
}