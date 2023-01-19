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
            return CreatedAtRoute(nameof(GetStory), new { storyId = serviceResponse.Data }, serviceResponse);
        }

        [HttpGet("admin-query")]
        public async Task<IActionResult> GetStories([FromQuery] int page, int pageSize)
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
            var serviceResponse = await _storyService.GetStories(domainName, page, pageSize);
            if (!serviceResponse.Success)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchStories([FromQuery] string searchValue, int page, int pageSize)
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
            var serviceResponse = await _storyService.SearchStories(searchValue, page, pageSize, domainName);
            if (!serviceResponse.Success)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }

        [HttpGet("{storyId}", Name = "GetStory")]
        public async Task<IActionResult> GetStory(Guid storyId)
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
            var response = await _storyService.GetStory(storyId, domainName);

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

        [HttpGet("titleId/{titleId}")]
        public async Task<IActionResult> GetStoryByTitleId(string titleId)
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
            var response = await _storyService.GetStoryByTitleId(titleId, domainName);

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

        // Calling route I keep on forgeting :) https://localhost:7400/api/Story/filter?type=nesto&category=bastina&page=1
        [HttpGet("filter")]
        public async Task<IActionResult> GetStoriesByCategory([FromQuery] string type, [FromQuery] Category category, [FromQuery] int page)
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;

            return Ok(await _storyService.GetStoriesByCategoryPaged(type, category, page, domainName));
        }

        [HttpGet("page")]
        public async Task<IActionResult> GetStoriesPaged([FromQuery] string type, [FromQuery] int page)
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
            return Ok(await _storyService.GetStoriesPaged(type, page, domainName));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStory(StoryUpdateDto storyUpdateDto)
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
            var serviceResponse = await _storyService.UpdateStory(storyUpdateDto, domainName);
            if (!serviceResponse.Success)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }

        [HttpDelete("{storyId}")]
        public async Task<IActionResult> DeleteStory(Guid storyId)
        {
            var response = await _storyService.DeleteStory(storyId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}