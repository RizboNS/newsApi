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
            CreateMap<ImageDto, ImageDb>();
            CreateMap<Story, StoryResponseDto>()
                .ForMember(dto => dto.TagNames, opt => opt.MapFrom(s => s.StoryTags.Select(st => st.Tag.TagName)));
        }
    }
}