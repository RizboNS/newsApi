namespace newsApi.Dtos
{
    public class ImageDto
    {
        public Guid Id { get; set; }
        public string LocationPath { get; set; } = string.Empty;
        public Guid StoryId { get; set; }
    }
}