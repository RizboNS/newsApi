﻿using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.StoryService
{
    public interface IStoryService
    {
        Task<ServiceResponse<StoryCreatedDto>> CreateStory(StoryCreateDto storyCreateDto, string domainName);

        Task<ServiceResponse<List<StoryResponseDto>>> GetStories();

        Task<ServiceResponse<StoryResponseDto>> GetStory(Guid storyId, string domainName);

        Task<ServiceResponse<StoryResponsePagedDto>> GetStoriesByCategoryPaged(Category category, int page);

        Task<ServiceResponse<StoryResponseDto>> UpdateStory(StoryUpdateDto storyUpdateDto, string domainName);

        Task<MethodResponse> DeleteStory(Guid storyId);
    }
}