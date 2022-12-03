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
            var serviceResponse = new ServiceResponse<StoryCreatedDto>();
            var storyCreatedDto = new StoryCreatedDto();
            storyCreatedDto.Id = Guid.NewGuid();
            var savedImages = new List<ImageSavedDto>();

            foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//img[@src]"))
            {
                HtmlAttribute att = link.Attributes["src"];
                var url = att.Value;
                string[] split01 = url.Split(",");
                if (split01.Length > 1)
                {
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
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Invalid image fromat.";
                    return serviceResponse;
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
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            var response = await _imageService.CreateImages(savedImages, storyCreatedDto.Id);
            if (!response.Success)
            {
                serviceResponse.Success = response.Success;
                serviceResponse.Message = response.Message;
                return serviceResponse;
            }

            serviceResponse.Data = storyCreatedDto;
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<StoryResponseDto>>> GetStories()
        {
            var serviceResponse = new ServiceResponse<List<StoryResponseDto>>();
            try
            {
                var stories = await _context.Stories
                    .Include(s => s.ImageDbs)
                    .ToListAsync();

                serviceResponse.Data = _mapper.Map<List<Story>, List<StoryResponseDto>>(stories);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public Task<ServiceResponse<List<StoryResponseDto>>> GetStoriesByCategory(Category category)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<StoryResponseDto>> GetStory(Guid storyId)
        {
            var serviceResponse = new ServiceResponse<StoryResponseDto>();

            try
            {
                var story = await _context.Stories
                    .Include(s => s.ImageDbs)
                    .FirstOrDefaultAsync(s => s.Id == storyId);

                if (story == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Story not found.";
                    return serviceResponse;
                }

                serviceResponse.Data = _mapper.Map<StoryResponseDto>(story);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<StoryResponseDto>> UpdateStory(StoryUpdateDto storyUpdateDto, string domainName)
        {
            var serviceResponse = new ServiceResponse<StoryResponseDto>();
            var story = await _context.Stories.FindAsync(storyUpdateDto.Id);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(storyUpdateDto.HtmlData);
            var savedImages = new List<ImageSavedDto>();

            foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//img[@src]"))
            {
                HtmlAttribute att = link.Attributes["src"];
                var url = att.Value;
                string[] split01 = url.Split(",");
                if (split01.Length > 1)
                {
                    var imageAsBase64 = split01[1];
                    var imageFileType = string.Empty;
                    if (split01[0].Contains("image"))
                    {
                        string[] split02 = split01[0].Split("/");
                        string[] split03 = split02[1].Split(";");
                        imageFileType = split03[0];
                    }
                    var res = await _imageService.SaveImage(imageAsBase64, imageFileType, storyUpdateDto.Id, storyUpdateDto.Category);
                    if (res.Success == true && res.Data != null)
                    {
                        res.Data.LocationDomain = domainName;
                        savedImages.Add(res.Data);

                        att.Value = res.Data.LocationDomain + res.Data.LocationPath;
                    }
                }
                else
                {
                    // TASK
                    // Check if image exist on this server, if not exist download it and save it to this server.
                    Console.WriteLine(url);
                    Console.WriteLine(domainName);

                    var requestDomain = url.Split("/")[2];
                    var domain = domainName.Split("/")[2];
                    Console.WriteLine(requestDomain);
                    Console.WriteLine(domain);
                    // Compare if domains match - if not check on server for existing image - if exists do nothing - if not upload it
                    // if not exists on server download it from the link and then upload it.
                    // change HTML body etc.
                }
            }

            return serviceResponse;
        }
    }
}