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

        [HttpPut]
        public async Task<IActionResult> ModifyTags(List<Tag> tags)
        {
            var serviceResponse = await _tagService.ModifyTags(tags);
            if (!serviceResponse.Success)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTags(List<Tag> tags)
        {
            var serviceResponse = await _tagService.CreateTags(tags);
            if (serviceResponse.Data is null || !serviceResponse.Success)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }

        [HttpGet]
        public async Task<IActionResult> GetTags()
        {
            var serviceResponse = await _tagService.GetTags();
            if (!serviceResponse.Success)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTags(List<Tag> tags)
        {
            var serviceResponse = await _tagService.DeleteTags(tags);
            if (!serviceResponse.Success)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }
    }
}