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
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(storyCreateDto.HtmlData);
            var serviceResponse = new ServiceResponse<StoryCreatedDto>();
            var storyCreatedDto = new StoryCreatedDto();
            storyCreatedDto.Id = Guid.NewGuid();
            var savedImages = new List<ImageDto>();
            var imgNodes = htmlDoc.DocumentNode.SelectNodes("//img[@src]");

            if (imgNodes != null)
            {
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
                        if (res.Success && res.Data != null)
                        {
                            savedImages.Add(res.Data);

                            att.Value = domainName + res.Data.LocationPath;
                        }
                    }
                    else
                    {
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

                            var res = await _imageService.SaveImage(imageAsBase64, imageFileType, storyCreatedDto.Id, storyCreateDto.Category);
                            if (res.Success == true && res.Data != null)
                            {
                                savedImages.Add(res.Data);
                                att.Value = domainName + res.Data.LocationPath;
                            }
                        }
                    }
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
            storyCreatedDto.Publish = storyCreateDto.Publish;

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

            var response1 = await _imageService.CreateImages(savedImages, storyCreatedDto.Id);
            if (!response1.Success)
            {
                serviceResponse.Success = response1.Success;
                serviceResponse.Message = response1.Message;
                return serviceResponse;
            }

            serviceResponse.Data = storyCreatedDto;
            return serviceResponse;
        }

        public async Task<MethodResponse> DeleteStory(Guid storyId)
        {
            var methodResponse = new MethodResponse();
            var story = _context.Stories.FirstOrDefault(s => s.Id == storyId);
            if (story == null)
            {
                methodResponse.Success = false;
                methodResponse.Message = "Story not found";
            }
            else
            {
                try
                {
                    await _imageService.DeleteImagesFromStory(storyId);
                    _context.Remove(story);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    methodResponse.Success = false;
                    methodResponse.Message = ex.Message;
                }
            }
            return methodResponse;
        }

        public async Task<ServiceResponse<List<StoryResponseDto>>> GetStories(string domainName)
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

        public async Task<ServiceResponse<StoryResponsePagedDto>> GetStoriesPaged(int page, string domainName)
        {
            var serviceResponse = new ServiceResponse<StoryResponsePagedDto>();
            var pageResult = 10f;
            var pageCount = Math.Ceiling(_context.Stories.Count() / pageResult);
            try
            {
                var stories = await _context.Stories
                    .Include(s => s.ImageDbs)
                    .OrderByDescending(s => s.PublishTime)
                    .Skip((page - 1) * (int)pageResult)
                    .Take((int)pageResult)
                    .ToListAsync();

                serviceResponse.Data = new StoryResponsePagedDto
                {
                    Stories = _mapper.Map<List<Story>, List<StoryResponseDto>>(stories),
                    PageCount = (int)pageCount,
                    PageSize = (int)pageResult,
                    Page = page
                };
                foreach (var story in serviceResponse.Data.Stories)
                {
                    if (story.ImageDbs != null)
                    {
                        foreach (var image in story.ImageDbs)
                        {
                            image.LocationPath = domainName + image.LocationPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<StoryResponsePagedDto>> GetStoriesByCategoryPaged(Category category, int page, string domainName)
        {
            var serviceResponse = new ServiceResponse<StoryResponsePagedDto>();
            var pageResult = 10f;
            var pageCount = Math.Ceiling(_context.Stories.Where(s => s.Category == category).Count() / pageResult);
            try
            {
                var stories = await _context.Stories
                    .Include(s => s.ImageDbs)
                    .Where(s => s.Category == category)
                    .OrderByDescending(s => s.PublishTime)
                    .Skip((page - 1) * (int)pageResult)
                    .Take((int)pageResult)
                    .ToListAsync();

                serviceResponse.Data = new StoryResponsePagedDto
                {
                    Stories = _mapper.Map<List<Story>, List<StoryResponseDto>>(stories),
                    PageCount = (int)pageCount,
                    PageSize = (int)pageResult,
                    Page = page
                };
                foreach (var story in serviceResponse.Data.Stories)
                {
                    if (story.ImageDbs != null)
                    {
                        foreach (var image in story.ImageDbs)
                        {
                            image.LocationPath = domainName + image.LocationPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<StoryResponseDto>> GetStory(Guid storyId, string domainName)
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
            var savedImages = new List<ImageDto>();
            var story = await _context.Stories
                .Include(s => s.ImageDbs)
                .FirstOrDefaultAsync(s => s.Id == storyUpdateDto.Id);
            if (story == null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Story not found.";
                return serviceResponse;
            }

            bool categoryChanged = story.Category != storyUpdateDto.Category;

            var htmlDocOld = new HtmlDocument();
            htmlDocOld.LoadHtml(story.HtmlData);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(storyUpdateDto.HtmlData);

            var imgNodesOld = htmlDocOld.DocumentNode.SelectNodes("//img[@src]");
            var imgNodes = htmlDoc.DocumentNode.SelectNodes("//img[@src]");

            await CompareHtmls(imgNodesOld, imgNodes);

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
                            savedImages.Add(res.Data);

                            att.Value = domainName + res.Data.LocationPath;
                        }
                    }
                    else
                    {
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
                                savedImages.Add(res.Data);
                                att.Value = domainName + res.Data.LocationPath;
                            }
                        }
                        else
                        {
                            if (categoryChanged)
                            {
                                // Check if image is in database
                                var urlLocalPathWithoutFirstSlash = urlLocalPath.Substring(1);
                                var res = await _imageService.ImageExists(urlLocalPathWithoutFirstSlash);
                                if (res.Success)
                                {
                                    // Check if image is on the server. If yes move it to the new category dir.
                                    if (File.Exists(serverPath))
                                    {
                                        var response = await _imageService.MoveImageToNewCategory(urlLocalPathWithoutFirstSlash, storyUpdateDto.Category);
                                        if (!response.Success)
                                        {
                                            serviceResponse.Success = response.Success;
                                            serviceResponse.Message = response.Message;
                                            return serviceResponse;
                                        }
                                        if (response.Data != null)
                                        {
                                            att.Value = domainName + response.Data.LocationPath;
                                        }
                                    }
                                }
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
            story.Publish = storyUpdateDto.Publish;
            story.UpdateTime = DateTime.Now;

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

            var response1 = await _imageService.CreateImages(savedImages, story.Id);
            if (!response1.Success)
            {
                serviceResponse.Success = response1.Success;
                serviceResponse.Message = response1.Message;
                return serviceResponse;
            }

            return serviceResponse;
        }

        private async Task CompareHtmls(HtmlNodeCollection oldDoc, HtmlNodeCollection newDoc)
        {
            if (oldDoc == null)
            {
                return;
            }
            if (newDoc == null)
            {
                // loop oldDoc and delete images...
                foreach (var linkOld in oldDoc)
                {
                    var urlOld = linkOld.Attributes["src"].Value;
                    Uri uri = new Uri(urlOld);
                    var file = Path.GetFileName(uri.LocalPath);
                    var fileId = new Guid(file.Split(".")[0]);
                    await _imageService.DeleteImage(fileId);
                }
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

        private string GetFileExtension(string base64Image)
        {
            string[] split01 = base64Image.Split(",");
            if (split01.Length > 1)
            {
                string[] split02 = split01[0].Split("/");
                string[] split03 = split02[1].Split(";");
                return split03[0];
            }
            return string.Empty;
        }

        private string GetPathWithoutDomain(string path)
        {
            if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                path = new Uri(path).LocalPath;
                while (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }
            }

            return path;
        }
    }
}