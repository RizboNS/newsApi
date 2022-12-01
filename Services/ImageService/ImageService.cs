using AutoMapper;
using ImageMagick;
using newsApi.Data;
using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.ImageService
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IMapper _mapper;
        private readonly DataContext _context;

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
                images.ForEach(image =>
                {
                    ImageDb imageDb = _mapper.Map<ImageDb>(image);
                    imageDb.StoryId = storyId;
                    _context.Add(imageDb);
                    imageDbs.Add(imageDb);
                });

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

        public async Task<ServiceResponse<ImageSavedDto>> SaveImage(string imageAsBase64, string imageFileType, Guid storyId, string storyCategory)
        {
            var serviceResponse = new ServiceResponse<ImageSavedDto>();
            var imageSaveDto = new ImageSavedDto();

            imageSaveDto.Id = Guid.NewGuid();

            var fileName = imageSaveDto.Id + "." + imageFileType;
            var filePath = GetFilePath(storyId, storyCategory);

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
                imageSaveDto.Location = GetPartialPath(storyId, storyCategory) + "/" + fileName;
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
    }
}