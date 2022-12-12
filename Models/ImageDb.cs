namespace newsApi.Models
{
    public class ImageDb
    {
        public Guid Id { get; set; }
        public string LocationPath { get; set; } = string.Empty;
        public Guid StoryId { get; set; }
    }
}