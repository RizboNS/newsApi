﻿using AutoMapper;
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

            var iconExtension = GetFileExtension(storyCreateDto.Icon);

            if (iconExtension == string.Empty)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Icon extension is not valid";
                return serviceResponse;
            }
            var response = await _imageService.SaveImage(storyCreateDto.Icon.Split(",")[1], iconExtension, storyCreatedDto.Id, storyCreateDto.Category);
            if (response.Success && response.Data != null)
            {
                storyCreatedDto.IconPath = response.Data.LocationPath;
                savedImages.Add(response.Data);
            }
            else
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Icon could not be saved";
                return serviceResponse;
            }

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
                foreach (var story in serviceResponse.Data)
                {
                    story.IconPath = domainName + story.IconPath;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<StoryResponsePagedDto>> GetStoriesByCategoryPaged(Category category, int page)
        {
            var serviceResponse = new ServiceResponse<StoryResponsePagedDto>();
            var pageResult = 4f; // Change to 10 for production
            var pageCount = Math.Ceiling(_context.Stories.Where(s => s.Category == category).Count() / pageResult);
            try
            {
                var stories = await _context.Stories
                    .Include(s => s.ImageDbs)
                    .Where(s => s.Category == category)
                    .OrderByDescending(s => s.CreatedTime) // Change to Publish time when set.
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
                story.IconPath = domainName + story.IconPath;
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

            // Check if story category is changed and move icon image to new folder
            if (story.Category != storyUpdateDto.Category)
            {
                var res = await _imageService.MoveImageToNewCategory(story.IconPath, storyUpdateDto.Category);
                if (!res.Success)
                {
                    serviceResponse.Success = res.Success;
                    serviceResponse.Message = res.Message;
                    return serviceResponse;
                }
                if (res.Data != null)
                {
                    story.IconPath = res.Data.LocationPath;
                }
            }

            var htmlDocOld = new HtmlDocument();
            htmlDocOld.LoadHtml(story.HtmlData);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(storyUpdateDto.HtmlData);

            var savedImages = new List<ImageDto>();

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

            var response1 = await _imageService.CreateImages(savedImages, story.Id);
            if (!response1.Success)
            {
                serviceResponse.Success = response1.Success;
                serviceResponse.Message = response1.Message;
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
    }
}

//UPDATE

//URL Could be outside of server - Download and Save - OK
//URL Could be from server (already exist) -Do nothing
//URL Could be from server but with old domain ( already exists ) -Update domain to current one
//URL Could be Encoded already (new image) -Save - OK