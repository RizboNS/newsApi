namespace newsApi.Dtos
{
    public class StoryCreatedDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string HtmlData { get; set; } = string.Empty;
        //public DateTime PublishTime { get; set; }
        //public DateTime CreatedTime { get; set; }
        //public DateTime UpdateTime { get; set; }
        //public List<ImageSavedDto>? ImageSavedDtos { get; set; }
    }
}