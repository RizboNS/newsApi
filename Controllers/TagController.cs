using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newsApi.Models;
using newsApi.Services.TagService;

namespace newsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTag(Tag tag)
        {
            var serviceResponse = await _tagService.CreateTag(tag);
            if (serviceResponse.Data == null)
            {
                return BadRequest(serviceResponse);
            }
            else if (serviceResponse.Success == false)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }
    }
}