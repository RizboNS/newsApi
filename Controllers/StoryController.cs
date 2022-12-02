using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newsApi.Dtos;
using newsApi.Services.StoryService;

namespace newsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoryController : ControllerBase
    {
        private readonly IStoryService _storyService;

        public StoryController(IStoryService storyService)
        {
            _storyService = storyService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory(StoryCreateDto storyCreateDto)
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;

            return Ok(await _storyService.CreateStory(storyCreateDto, domainName));
        }

        [HttpGet]
        public async Task<IActionResult> GetStories()
        {
            return Ok(await _storyService.GetStories());
        }
    }
}