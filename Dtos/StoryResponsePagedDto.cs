namespace newsApi.Dtos
{
    public class StoryResponsePagedDto
    {
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public List<StoryResponseDto> Stories { get; set; } = new List<StoryResponseDto>();
    }
}