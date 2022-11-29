namespace newsApi.Models
{
    public class ImageDb
    {
        public Guid Id { get; set; }
        public string Location { get; set; } = string.Empty;
        public ImageClass ImageClass { get; set; }
        public Guid StoryId { get; set; }
    }
}