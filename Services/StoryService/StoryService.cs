﻿using HtmlAgilityPack;
using newsApi.Dtos;
using newsApi.Models;
using newsApi.Services.ImageService;

namespace newsApi.Services.StoryService
{
    public class StoryService : IStoryService
    {
        private readonly IImageService _imageService;

        public StoryService(IImageService imageService)
        {
            _imageService = imageService;
        }

        public async Task<ServiceResponse<StoryCreatedDto>> CreateStory(StoryCreateDto storyCreateDto)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(storyCreateDto.HtmlData);
            var serviceReponse = new ServiceResponse<StoryCreatedDto>();
            var storyCreatedDto = new StoryCreatedDto();
            var savedImages = new List<ImageSavedDto>();

            var urls = htmlDoc.DocumentNode.Descendants("img")
                .Select(e => e.GetAttributeValue("src", null))
                .Where(s => !string.IsNullOrEmpty(s));

            var imageFileType = string.Empty;
            var imageAsBase64 = string.Empty;

            //foreach (var url in urls)
            //{
            //    string[] split01 = url.Split(",");
            //    imageAsBase64 = split01[1];
            //    if (split01[0].Contains("image"))
            //    {
            //        string[] split02 = split01[0].Split("/");
            //        string[] split03 = split02[1].Split(";");
            //        imageFileType = split03[0];
            //    }
            //    var response = await _imageService.SaveImage(imageAsBase64, imageFileType);
            //    if (response.Success == true && response.Data != null)
            //    {
            //        savedImages.Add(response.Data);
            //    }
            //}
            foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//img[@src]"))
            {
                HtmlAttribute att = link.Attributes["src"];
                var url = att.Value;
                string[] split01 = url.Split(",");
                imageAsBase64 = split01[1];
                if (split01[0].Contains("image"))
                {
                    string[] split02 = split01[0].Split("/");
                    string[] split03 = split02[1].Split(";");
                    imageFileType = split03[0];
                }
                var response = await _imageService.SaveImage(imageAsBase64, imageFileType);
                if (response.Success == true && response.Data != null)
                {
                    savedImages.Add(response.Data);

                    att.Value = "newTestvalue";
                }
            }
            //htmlDoc.Save(); TO DO SAVE USING STREAM PROBABLY

            storyCreatedDto.HtmlData = "TODO";
            storyCreatedDto.ImageSavedDtos = savedImages;
            serviceReponse.Data = storyCreatedDto;

            return serviceReponse;
        }
    }
}