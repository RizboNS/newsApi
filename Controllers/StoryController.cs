using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newsApi.Dtos;
using newsApi.Models;
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
            var serviceResponse = await _storyService.CreateStory(storyCreateDto, domainName);
            if (!serviceResponse.Success)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }

        [HttpGet]
        public async Task<IActionResult> GetStories()
        {
            return Ok(await _storyService.GetStories());
        }

        [HttpGet("{storyId}")]
        public async Task<IActionResult> GetStory(Guid storyId)
        {
            Console.WriteLine("ran");
            var response = await _storyService.GetStory(storyId);

            if (response.Data == null)
            {
                return NotFound(response);
            }
            else if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStory(StoryUpdateDto storyUpdateDto)
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
            return Ok(await _storyService.UpdateStory(storyUpdateDto, domainName));
        }
    }
}