﻿using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.StoryService
{
    public interface IStoryService
    {
        Task<ServiceResponse<StoryCreatedDto>> CreateStory(StoryCreateDto storyCreateDto);
    }
}