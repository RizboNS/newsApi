﻿using Microsoft.AspNetCore.Http;
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
            if (serviceResponse.Data == null)
            {
                return BadRequest(serviceResponse);
            }
            return CreatedAtRoute(nameof(GetStory), new { storyId = serviceResponse.Data.Id }, serviceResponse);
        }

        [HttpGet]
        public async Task<IActionResult> GetStories()
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
            return Ok(await _storyService.GetStories(domainName));
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

        // Calling route I keep on forgeting :) https://localhost:7400/api/Story/category?category=bastina&page=1
        [HttpGet("category")]
        public async Task<IActionResult> GetStoriesByCategory([FromQuery] Category category, [FromQuery] int page)
        {
            return Ok(await _storyService.GetStoriesByCategoryPaged(category, page));
        }

        [HttpGet("page")]
        public async Task<IActionResult> GetStoriesPaged([FromQuery] int page)
        {
            return Ok(await _storyService.GetStoriesPaged(page));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStory(StoryUpdateDto storyUpdateDto)
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
            var serviceResponse = await _storyService.UpdateStory(storyUpdateDto, domainName);
            if (serviceResponse.Data == null)
            {
                return BadRequest(serviceResponse);
            }
            if (!serviceResponse.Success)
            {
                return NotFound(serviceResponse);
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