using AutoMapper;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using newsApi.Data;
using newsApi.Dtos;
using newsApi.Models;
using newsApi.Services.ImageService;

namespace newsApi.Services.StoryService
{
    public class StoryService : IStoryService
    {
        private readonly IImageService _imageService;
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public StoryService(
            IImageService imageService, DataContext context, IMapper mapper)
        {
            _imageService = imageService;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<StoryCreatedDto>> CreateStory(StoryCreateDto storyCreateDto, string domainName)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(storyCreateDto.HtmlData);
            var serviceReponse = new ServiceResponse<StoryCreatedDto>();
            var storyCreatedDto = new StoryCreatedDto();
            storyCreatedDto.Id = Guid.NewGuid();
            var savedImages = new List<ImageSavedDto>();

            foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//img[@src]"))
            {
                HtmlAttribute att = link.Attributes["src"];
                var url = att.Value;
                string[] split01 = url.Split(",");
                var imageAsBase64 = split01[1];
                var imageFileType = string.Empty;
                if (split01[0].Contains("image"))
                {
                    string[] split02 = split01[0].Split("/");
                    string[] split03 = split02[1].Split(";");
                    imageFileType = split03[0];
                }
                var res = await _imageService.SaveImage(imageAsBase64, imageFileType, storyCreatedDto.Id, storyCreateDto.Category);
                if (res.Success == true && res.Data != null)
                {
                    res.Data.LocationDomain = domainName;
                    savedImages.Add(res.Data);

                    att.Value = res.Data.LocationDomain + res.Data.LocationPath;
                }
            }

            MemoryStream memoryStream = new MemoryStream();
            htmlDoc.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            StreamReader streamReader = new StreamReader(memoryStream);

            storyCreatedDto.HtmlData = streamReader.ReadToEnd();
            storyCreatedDto.Description = storyCreateDto.Description;
            storyCreatedDto.Category = storyCreateDto.Category;
            storyCreatedDto.Title = storyCreateDto.Title;
            storyCreatedDto.PublishTime = storyCreateDto.PublishTime;

            Story story = _mapper.Map<Story>(storyCreatedDto);

            try
            {
                _context.Add(story);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                serviceReponse.Success = false;
                serviceReponse.Message = ex.Message;
            }

            var response = await _imageService.CreateImages(savedImages, storyCreatedDto.Id);
            if (!response.Success)
            {
                serviceReponse.Success = response.Success;
                serviceReponse.Message = response.Message;
                return serviceReponse;
            }

            serviceReponse.Data = storyCreatedDto;
            return serviceReponse;
        }

        public async Task<ServiceResponse<List<Story>>> GetStories()
        {
            var serviceResponse = new ServiceResponse<List<Story>>();

            var stories = await _context.Stories.ToListAsync();

            serviceResponse.Data = stories;

            return serviceResponse;
        }
    }
}