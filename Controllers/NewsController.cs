using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newsApi.Models;
using System.Drawing;
using System.Text;

namespace newsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateNews(TestModel testmodel)
        {
            //Console.WriteLine(testmodel.HtmlData);
            CreateImage();
            return Ok(testmodel);
        }

        public void CreateImage(string strBase64, string fileType)
        {
            Guid id = Guid.NewGuid();
            byte[] bytes = Convert.FromBase64String(strBase64);

            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only
            // Read and try to change lib - as I don't know server env this app will be deployed.
            image.Save("./Images/" + id.ToString() + fileType);
        }
    }
}