﻿using AutoMapper;
using newsApi.Dtos;
using newsApi.Models;

namespace newsApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<StoryCreatedDto, Story>();
            CreateMap<ImageDto, ImageDb>();
            CreateMap<Story, StoryResponseDto>();
        }
    }
}