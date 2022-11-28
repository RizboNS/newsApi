using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newsApi.Models;
using ImageMagick;

namespace newsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateNews(TestModel testmodel)
        {
            ParseHtml(testmodel);
            return Ok(testmodel);
        }

        private static void ParseHtml(TestModel testmodel)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(testmodel.HtmlData);

            var urls = doc.DocumentNode.Descendants("img")
                .Select(e => e.GetAttributeValue("src", null))
                .Where(s => !string.IsNullOrEmpty(s));

            var fileType = string.Empty;
            var strBase64 = string.Empty;

            foreach (var url in urls)
            {
                string[] split01 = url.Split(",");
                strBase64 = split01[1];
                if (split01[0].Contains("image"))
                {
                    string[] split02 = split01[0].Split("/");
                    string[] split03 = split02[1].Split(";");
                    fileType = split03[0];
                }
                SaveImage(strBase64, fileType);
            }
        }

        private static async void SaveImage(string strBase64, string fileType)
        {
            Console.WriteLine("ran");
            Guid id = Guid.NewGuid();
            byte[] bytes = Convert.FromBase64String(strBase64);

            var image = new MagickImage(bytes);

            switch (fileType)
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
            //var size = new MagickGeometry(maxWidth, maxHeight);
            //image.Resize(size);
            await image.WriteAsync($"./Images/articleImage{id}.{fileType}");
        }
    }
}