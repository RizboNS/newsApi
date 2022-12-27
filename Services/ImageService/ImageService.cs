using AutoMapper;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ServiceResponse<List<ImageDb>>> CreateImages(List<ImageDto> images, Guid storyId)
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

        public async Task<ServiceResponse<ImageDto>> SaveImage(string imageAsBase64, string imageFileType, Guid storyId, Category storyCategory)
        {
            var serviceResponse = new ServiceResponse<ImageDto>();
            var imageSaveDto = new ImageDto();

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
                        var dir = _environment.WebRootPath + "/" + imageToDelete.LocationPath;
                        dir = Path.GetDirectoryName(dir);
                        if (Directory.Exists(dir))
                        {
                            bool isDirPathEmpty = !Directory.EnumerateFileSystemEntries(dir).Any();
                            if (isDirPathEmpty)
                            {
                                Directory.Delete(dir, true);
                            }
                        }
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

        public async Task<MethodResponse> DeleteImagesFromStory(Guid storyId)
        {
            var methodResponse = new MethodResponse();
            var imagesDb = await _context.ImageDbs
                .Where(i => i.StoryId == storyId)
                .ToListAsync();

            try
            {
                var folder = string.Empty;
                if (imagesDb.Count > 0 || imagesDb != null)
                {
                    folder = _environment.WebRootPath + "/" + imagesDb[0].LocationPath.Substring(0, imagesDb[0].LocationPath.LastIndexOf('/'));
                    foreach (var image in imagesDb)
                    {
                        methodResponse = deleteFromSystem(image.LocationPath);
                    }
                    if (methodResponse.Success)
                    {
                        _context.ImageDbs.RemoveRange(imagesDb);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    methodResponse.Success = false;
                    methodResponse.Message = "Images not found.";
                }

                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (Exception ex)
            {
                methodResponse.Success = false;
                methodResponse.Message = ex.Message;
            }

            return methodResponse;
        }

        public async Task<ServiceResponse<ImageDto>> MoveImageToNewCategory(string path, Category newCategory)
        {
            var serviceResponse = new ServiceResponse<ImageDto>();
            var imageDto = new ImageDto();
            var imageDb = await _context.ImageDbs
                .Where(i => i.LocationPath == path)
                .FirstOrDefaultAsync();
            if (imageDb == null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Image not found at the Database.";
                return serviceResponse;
            }

            var oldPath = _environment.WebRootPath + "/" + path;
            var fileName = Path.GetFileName(oldPath);
            var dirPath = Path.GetDirectoryName(oldPath);
            var partialPath = GetPartialPath(imageDb.StoryId, newCategory.ToString());
            var newPath = _environment.WebRootPath + "/" + partialPath;
            try
            {
                if (File.Exists(oldPath))
                {
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                    File.Move(oldPath, newPath + "/" + path.Split('/').Last());
                    imageDto.LocationPath = partialPath + "/" + fileName;
                    imageDb.LocationPath = partialPath + "/" + fileName;
                    serviceResponse.Data = imageDto;

                    if (Directory.Exists(dirPath))
                    {
                        bool isDirPathEmpty = !Directory.EnumerateFileSystemEntries(dirPath).Any();
                        if (isDirPathEmpty)
                        {
                            Directory.Delete(dirPath, true);
                        }
                    }
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "File does not exist.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<MethodResponse> ImageExists(string path)
        {
            var response = new MethodResponse();

            try
            {
                var image = await _context.ImageDbs.FirstOrDefaultAsync(i => i.LocationPath == path);
                if (image == null)
                {
                    response.Success = false;
                    response.Message = "Image does not exist.";
                    return response;
                }
            }
            catch (Exception error)
            {
                response.Success = false;
                response.Message = error.Message;
                return response;
            }

            return response;
        }

        public async Task<ServiceResponse<Guid>> GetIconId(string path)
        {
            var res = new ServiceResponse<Guid>();
            var imageDb = await _context.ImageDbs.FirstOrDefaultAsync(i => i.LocationPath == path);
            if (imageDb == null)
            {
                res.Success = false;
                res.Message = "Image not found.";
                return res;
            }
            res.Data = imageDb.Id;
            return res;
        }
    }
}