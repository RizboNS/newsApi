using AutoMapper;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using newsApi.Data;
using newsApi.Dtos;
using newsApi.Models;
using newsApi.Services.ImageService;
using newsApi.Services.TagService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Resources;
using System.Runtime.Intrinsics.X86;
using System;

namespace newsApi.Services.StoryService
{
    public class StoryService : IStoryService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IImageService _imageService;
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ITagService _tagService;

        public StoryService(IWebHostEnvironment environment,
            IImageService imageService, DataContext context, IMapper mapper, ITagService tagService)
        {
            _environment = environment;
            _imageService = imageService;
            _context = context;
            _mapper = mapper;
            _tagService = tagService;
        }

        public async Task<ServiceResponse<Guid>> CreateStory(StoryCreateDto storyCreateDto, string domainName)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(storyCreateDto.HtmlData);
            var serviceResponse = new ServiceResponse<Guid>();
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

            if (storyCreateDto.Tags is not null)
            {
                var methodResponse = await _tagService.CheckTagsAndCreateIfNotExist(storyCreateDto.Tags, storyCreatedDto.Id);
                if (!methodResponse.Success)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = methodResponse.Message;
                    return serviceResponse;
                }
            }
            else
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Tags are required.";
                return serviceResponse;
            }

            MemoryStream memoryStream = new MemoryStream();
            htmlDoc.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            StreamReader streamReader = new StreamReader(memoryStream);

            storyCreatedDto.HtmlData = streamReader.ReadToEnd();
            storyCreatedDto.Category = storyCreateDto.Category;
            storyCreatedDto.Title = storyCreateDto.Title;
            storyCreatedDto.Type = storyCreateDto.Type;
            storyCreatedDto.PublishTime = storyCreateDto.PublishTime;
            storyCreatedDto.Publish = storyCreateDto.Publish;
            storyCreatedDto.TitleId = CreateTitleId(storyCreatedDto.Title);

            Story story = _mapper.Map<Story>(storyCreatedDto);

            story.StoryTags = new List<StoryTag>();
            foreach (var tag in storyCreateDto.Tags)
            {
                story.StoryTags.Add(new StoryTag { StoryId = story.Id, TagName = tag.TagName });
            }

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

            serviceResponse.Data = story.Id;
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
                    .Include(s => s.StoryTags)
                    .ThenInclude(st => st.Tag)
                    .Include(s => s.ImageDbs)
                    .AsSplitQuery()
                    .ToListAsync();

                serviceResponse.Data = _mapper.Map<List<Story>, List<StoryResponseDto>>(stories);

                foreach (var story in serviceResponse.Data)
                {
                    if (story.ImageDbs != null)
                    {
                        for (int i = 0; i < story.ImageDbs.Count; i++)
                        {
                            var imageLocation = story.ImageDbs[i];
                            story.ImageDbs[i] = domainName + imageLocation;
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

        public async Task<ServiceResponse<StoryResponsePagedDto>> GetStoriesPaged(string type, int page, string domainName)
        {
            var serviceResponse = new ServiceResponse<StoryResponsePagedDto>();
            var pageResult = 10f;
            var pageCount = Math.Ceiling(_context.Stories.Where(s => type == "all" ? s.Type != "blog" && s.Type != "en-de" : s.Type == type).Count() / pageResult);
            try
            {
                var stories = await _context.Stories
                .Include(s => s.ImageDbs)
                .Where(s => type == "all" ? s.Type != "blog" && s.Type != "en-de" : s.Type == type)
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
                        for (int i = 0; i < story.ImageDbs.Count; i++)
                        {
                            var imageLocation = story.ImageDbs[i];
                            story.ImageDbs[i] = domainName + imageLocation;
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

        public async Task<ServiceResponse<StoryResponsePagedDto>> GetStoriesByCategoryPaged(string type, Category category, int page, string domainName)
        {
            var serviceResponse = new ServiceResponse<StoryResponsePagedDto>();
            var pageResult = 10f;
            var pageCount = Math.Ceiling(_context.Stories.Where(s => type == "all" ? s.Category == category : s.Category == category && s.Type == type).Count() / pageResult);
            try
            {
                var stories = await _context.Stories
                    .Include(s => s.ImageDbs)
                    .Where(s => type == "all" ? s.Category == category : s.Category == category && s.Type == type)
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
                        for (int i = 0; i < story.ImageDbs.Count; i++)
                        {
                            var imageLocation = story.ImageDbs[i];
                            story.ImageDbs[i] = domainName + imageLocation;
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

        public async Task<ServiceResponse<StoryResponseDto>> GetStoryByTitleId(string titleId, string domainName)
        {
            var serviceResponse = new ServiceResponse<StoryResponseDto>();
            try
            {
                var story = await _context.Stories
                    .Include(s => s.ImageDbs)
                    .FirstOrDefaultAsync(s => s.TitleId == titleId);

                serviceResponse.Data = _mapper.Map<StoryResponseDto>(story);
                if (serviceResponse.Data.ImageDbs is not null)
                {
                    // to be tested
                    for (int i = 0; i < serviceResponse.Data.ImageDbs.Count; i++)
                    {
                        var imageLocation = serviceResponse.Data.ImageDbs[i];
                        serviceResponse.Data.ImageDbs[i] = domainName + imageLocation;
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
                    .Include(s => s.StoryTags)
                    .ThenInclude(st => st.Tag)
                    .AsSplitQuery()
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

        public async Task<ServiceResponse<Guid>> UpdateStory(StoryUpdateDto storyUpdateDto, string domainName)
        {
            var serviceResponse = new ServiceResponse<Guid>();
            var savedImages = new List<ImageDto>();
            var story = await _context.Stories
                .Include(s => s.ImageDbs)
                .Include(s => s.StoryTags)
                .ThenInclude(st => st.Tag)
                .AsSplitQuery()
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

            if (storyUpdateDto.Tags is not null)
            {
                var methodResponse = await _tagService.CheckTagsAndCreateIfNotExist(storyUpdateDto.Tags, storyUpdateDto.Id);
                if (!methodResponse.Success)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = methodResponse.Message;
                    return serviceResponse;
                }
            }
            else
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Tags are required.";
                return serviceResponse;
            }

            story.HtmlData = streamReader.ReadToEnd();
            story.Category = storyUpdateDto.Category;
            story.Title = storyUpdateDto.Title;
            story.Type = storyUpdateDto.Type;
            story.PublishTime = storyUpdateDto.PublishTime;
            story.Publish = storyUpdateDto.Publish;
            story.UpdateTime = DateTime.Now;
            story.TitleId = CreateTitleId(story.Title);

            // check if story.StoryTags is not null
            if (story.StoryTags is null)
            {
                story.StoryTags = new List<StoryTag>();
            }
            // check if tags count is greater than 0 and then find if tags needs to be deleted...
            if (story.StoryTags.Count > 0)
            {
                var storyTagsCopy = new List<StoryTag>(story.StoryTags);
                foreach (var storyTag in storyTagsCopy)
                {
                    var tag = storyUpdateDto.Tags.FirstOrDefault(t => t.TagName == storyTag.TagName);
                    if (tag is null)
                    {
                        story.StoryTags.Remove(storyTag);
                    }
                }
            }

            // add tags if not exists
            foreach (var tag in storyUpdateDto.Tags)
            {
                // check if tag already exist in story.StoryTags
                var tagExists = story.StoryTags.Any(t => t.TagName == tag.TagName);
                if (!tagExists)
                {
                    story.StoryTags.Add(new StoryTag { StoryId = story.Id, TagName = tag.TagName });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                serviceResponse.Data = story.Id;
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

        private string CreateTitleId(string title)
        {
            return title.ToLower().Replace(" ", "-").Replace("?", "").Replace("!", "").Replace(":", "").Replace(";", "").Replace(",", "").Replace(".", "").Replace("(", "").Replace(")", "").Replace("č", "c").Replace("ć", "c").Replace("š", "s").Replace("ž", "z").Replace("đ", "dj");
        }
    }
}