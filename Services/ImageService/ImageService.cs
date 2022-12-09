using AutoMapper;
using ImageMagick;
using newsApi.Data;
using newsApi.Dtos;
using newsApi.Models;
using System.Net;

namespace newsApi.Services.ImageService
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private static readonly HttpClient _httpClient = new HttpClient();

        public ImageService(IWebHostEnvironment environment, IMapper mapper, DataContext context)
        {
            _environment = environment;
            _mapper = mapper;
            _context = context;
        }

        public async Task<ServiceResponse<List<ImageDb>>> CreateImages(List<ImageSavedDto> images, Guid storyId)
        {
            var serviceResponse = new ServiceResponse<List<ImageDb>>();
            var imageDbs = new List<ImageDb>();

            try
            {
                foreach (var image in images)
                {
                    var imageInDb = await _context.ImageDbs.FindAsync(image.Id);
                    if (imageInDb == null)
                    {
                        ImageDb imageDb = _mapper.Map<ImageDb>(image);
                        imageDb.StoryId = storyId;
                        _context.Add(imageDb);
                        imageDbs.Add(imageDb);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            serviceResponse.Data = imageDbs;

            return serviceResponse;
        }

        public async Task<ServiceResponse<string>> DownloadImageToBase64(string url)
        {
            var serviceResponse = new ServiceResponse<string>();
            var base64String = string.Empty;

            try
            {
                byte[] fileBytes = await _httpClient.GetByteArrayAsync(url);
                base64String = Convert.ToBase64String(fileBytes);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            serviceResponse.Data = base64String;
            return serviceResponse;
        }

        public async Task<ServiceResponse<ImageSavedDto>> SaveImage(string imageAsBase64, string imageFileType, Guid storyId, Category storyCategory)
        {
            var serviceResponse = new ServiceResponse<ImageSavedDto>();
            var imageSaveDto = new ImageSavedDto();

            imageSaveDto.Id = Guid.NewGuid();

            var fileName = imageFileType.Contains('.') ? imageSaveDto.Id + imageFileType : imageSaveDto.Id + "." + imageFileType;
            var filePath = GetFilePath(storyId, storyCategory.ToString());

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            var fullPath = $"{filePath}/{fileName}";

            byte[] bytes = Convert.FromBase64String(imageAsBase64);

            var image = new MagickImage(bytes);

            switch (imageFileType)
            {
                case "jpeg":
                    image.Format = MagickFormat.Jpeg;
                    break;

                case "png":
                    image.Format = MagickFormat.Png;
                    break;

                case "gif":
                    image.Format = MagickFormat.Gif;
                    break;

                default:
                    // Exit
                    break;
            }
            try
            {
                await image.WriteAsync(fullPath);
                imageSaveDto.LocationPath = GetPartialPath(storyId, storyCategory.ToString()) + "/" + fileName;
                serviceResponse.Data = imageSaveDto;
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        private string GetFilePath(Guid storyId, string storyCategory)
        {
            return _environment.WebRootPath + "/Images/Story/" + storyCategory + "/" + storyId;
        }

        private string GetPartialPath(Guid storyId, string storyCategory)
        {
            return "Images/Story/" + storyCategory + "/" + storyId;
        }

        public string GetFileExtension(string url)
        {
            url = url.Split('?')[0];
            url = url.Split('/').Last();
            return url.Contains('.') ? url.Substring(url.LastIndexOf('.')) : "";
        }

        public async Task<ServiceResponse<bool>> DeleteImage(Guid imageId)
        {
            var serviceResponse = new ServiceResponse<bool>();
            var imageToDelete = await _context.ImageDbs.FindAsync(imageId);
            try
            {
                if (imageToDelete != null)
                {
                    var methodResponse = deleteFromSystem(imageToDelete.LocationPath);
                    if (methodResponse.Success)
                    {
                        _context.ImageDbs.Remove(imageToDelete);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = methodResponse.Message;
                    }
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Image not found.";
                    serviceResponse.Data = false;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.Data = false;
            }

            return serviceResponse;
        }

        private MethodResponse deleteFromSystem(string imagePath)
        {
            var methodResponse = new MethodResponse();
            try
            {
                var fileToDelete = _environment.WebRootPath + "/" + imagePath;
                if (File.Exists(fileToDelete))
                {
                    File.Delete(fileToDelete);
                }
                else
                {
                    methodResponse.Success = false;
                    methodResponse.Message = "File does not exist.";
                }
            }
            catch (Exception ex)
            {
                methodResponse.Success = false;
                methodResponse.Message = ex.Message;
            }
            return methodResponse;
        }
    }
}