using HtmlAgilityPack;
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

        public async void ParseHtml(string storyAsHtmlString)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(storyAsHtmlString);
            var savedImages = new List<ImageSavedDto>();

            var urls = doc.DocumentNode.Descendants("img")
                .Select(e => e.GetAttributeValue("src", null))
                .Where(s => !string.IsNullOrEmpty(s));

            var imageFileType = string.Empty;
            var imageAsBase64 = string.Empty;

            foreach (var url in urls)
            {
                var serviceResponse = new ServiceResponse<ImageSavedDto>();
                string[] split01 = url.Split(",");
                imageAsBase64 = split01[1];
                if (split01[0].Contains("image"))
                {
                    string[] split02 = split01[0].Split("/");
                    string[] split03 = split02[1].Split(";");
                    imageFileType = split03[0];
                }
                serviceResponse = await _imageService.SaveImage(imageAsBase64, imageFileType);
                if (serviceResponse.Success == true && serviceResponse.Data != null)
                {
                    savedImages.Add(serviceResponse.Data);
                }
            }
        }
    }
}