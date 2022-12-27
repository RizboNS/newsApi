using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.ImageService
{
    public interface IImageService
    {
        Task<ServiceResponse<ImageDto>> SaveImage(string imageAsBase64, string imageFileType, Guid storyId, Category storyCategory);

        Task<ServiceResponse<List<ImageDb>>> CreateImages(List<ImageDto> images, Guid storyId);

        Task<ServiceResponse<string>> DownloadImageToBase64(string url);

        Task<ServiceResponse<bool>> DeleteImage(Guid imageId);

        Task<MethodResponse> DeleteImagesFromStory(Guid storyId);

        Task<ServiceResponse<ImageDto>> MoveImageToNewCategory(string path, Category newCategory);

        Task<MethodResponse> ImageExists(string path);

        Task<ServiceResponse<Guid>> GetIconId(string path);

        string GetFileExtension(string url);
    }
}