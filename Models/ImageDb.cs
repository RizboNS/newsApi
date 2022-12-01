namespace newsApi.Models
{
    public class ImageDb
    {
        public Guid Id { get; set; }
        public string Location { get; set; } = string.Empty;
        public Guid StoryId { get; set; }
    }
}