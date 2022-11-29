namespace newsApi.Models
{
    public class Story
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PublishTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public List<ImageDb>? ImageDbs { get; set; }
    }
}