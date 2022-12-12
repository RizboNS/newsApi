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
        private readonly IWebHostEnvironment _environment;
        private readonly IImageService _imageService;
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public StoryService(IWebHostEnvironment environment,
            IImageService imageService, DataContext context, IMapper mapper)
        {
            _environment = environment;
            _imageService = imageService;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<StoryCreatedDto>> CreateStory(StoryCreateDto storyCreateDto, string domainName)
        {
            // Add option to insert image from a link (similar to the method in update story).
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
            var story = await _context.Stories
                .Include(s => s.ImageDbs)
                .FirstOrDefaultAsync(s => s.Id == storyUpdateDto.Id);
            if (story == null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Story not found.";
                return serviceResponse;
            }
            var htmlDocOld = new HtmlDocument();
            htmlDocOld.LoadHtml(story.HtmlData);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(storyUpdateDto.HtmlData);

            var savedImages = new List<ImageSavedDto>();

            var imgNodesOld = htmlDocOld.DocumentNode.SelectNodes("//img[@src]");
            var imgNodes = htmlDoc.DocumentNode.SelectNodes("//img[@src]");

            await CompareHtmls(imgNodesOld, imgNodes, story);

            if (imgNodes != null)
            {
                foreach (HtmlNode link in imgNodes)
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
                        var urlHost = new Uri(url).Host;
                        var localHost = new Uri(domainName).Host;
                        var urlLocalPath = new Uri(url).LocalPath;
                        var serverPath = _environment.WebRootPath + "/" + urlLocalPath;
                        if (!File.Exists(serverPath))
                        {
                            var resDownload = await _imageService.DownloadImageToBase64(url);
                            if (resDownload.Data == null)
                            {
                                serviceResponse.Success = false;
                                serviceResponse.Message = resDownload.Message;
                                return serviceResponse;
                            }

                            var imageAsBase64 = resDownload.Data;
                            var imageFileType = _imageService.GetFileExtension(url);

                            var res = await _imageService.SaveImage(imageAsBase64, imageFileType, storyUpdateDto.Id, storyUpdateDto.Category);
                            if (res.Success == true && res.Data != null)
                            {
                                res.Data.LocationDomain = domainName;
                                savedImages.Add(res.Data);

                                att.Value = res.Data.LocationDomain + res.Data.LocationPath;
                            }
                        }
                    }
                }
            }

            MemoryStream memoryStream = new MemoryStream();
            htmlDoc.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            StreamReader streamReader = new StreamReader(memoryStream);

            story.HtmlData = streamReader.ReadToEnd();
            story.Description = storyUpdateDto.Description;
            story.Category = storyUpdateDto.Category;
            story.Title = storyUpdateDto.Title;
            story.PublishTime = storyUpdateDto.PublishTime;

            try
            {
                await _context.SaveChangesAsync();
                StoryResponseDto storyResponseDto = _mapper.Map<StoryResponseDto>(story);
                serviceResponse.Data = storyResponseDto;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            var response = await _imageService.CreateImages(savedImages, story.Id);
            if (!response.Success)
            {
                serviceResponse.Success = response.Success;
                serviceResponse.Message = response.Message;
                return serviceResponse;
            }

            return serviceResponse;
        }

        private async Task CompareHtmls(HtmlNodeCollection oldDoc, HtmlNodeCollection newDoc, Story story)
        {
            if (oldDoc == null)
            {
                return;
            }
            if (newDoc == null)
            {
                await _imageService.DeleteImagesFromStory(story.Id);
                return;
            }

            foreach (var linkOld in oldDoc)
            {
                var urlOld = linkOld.Attributes["src"].Value;
                var urlFound = false;
                foreach (var linkNew in newDoc)
                {
                    var urlNew = linkNew.Attributes["src"].Value;
                    if (urlNew == urlOld)
                    {
                        urlFound = true;
                    }
                }
                if (!urlFound)
                {
                    Uri uri = new Uri(urlOld);
                    var file = Path.GetFileName(uri.LocalPath);
                    var fileId = new Guid(file.Split(".")[0]);
                    await _imageService.DeleteImage(fileId);
                }
            }
        }
    }
}

//UPDATE

//URL Could be outside of server - Download and Save - OK
//URL Could be from server (already exist) -Do nothing
//URL Could be from server but with old domain ( already exists ) -Update domain to current one
//URL Could be Encoded already (new image) -Save - OK