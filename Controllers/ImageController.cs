using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace newsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        // https://localhost:7289//Images/birdy.png Example to work on it! It works!
        //private readonly IWebHostEnvironment _environment;

        //public ImageController(IWebHostEnvironment environment)
        //{
        //    _environment = environment;
        //}

        //[HttpGet]
        //public ActionResult GetImage()
        //{
        //}

        //private string GetFilePath()
        //{
        //    return _environment.WebRootPath + "\\Images\\birdy.png";
        //}
    }
}