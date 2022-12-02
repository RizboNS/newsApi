using AutoMapper;
using newsApi.Dtos;
using newsApi.Models;

namespace newsApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<StoryCreatedDto, Story>();
            CreateMap<ImageSavedDto, ImageDb>();
            CreateMap<Story, StoryResponseDto>();
        }
    }
}