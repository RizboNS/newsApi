﻿using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.StoryService
{
    public interface IStoryService
    {
        Task<ServiceResponse<StoryCreatedDto>> CreateStory(StoryCreateDto storyCreateDto, string domainName);

        Task<ServiceResponse<List<StoryResponseDto>>> GetStories();

        Task<ServiceResponse<StoryResponseDto>> GetStory(Guid storyId, string domainName);

        Task<ServiceResponse<List<StoryResponseDto>>> GetStoriesByCategory(Category category);

        Task<ServiceResponse<StoryResponseDto>> UpdateStory(StoryUpdateDto storyUpdateDto, string domainName);
    }
}