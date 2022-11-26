using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newsApi.Models;

namespace newsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateNews(TestModel testmodel)
        {
            Console.WriteLine(testmodel.HtmlData);
            return Ok(testmodel);
        }
    }
}