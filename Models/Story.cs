namespace newsApi.Models
{
    public class Story
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HtmlData { get; set; } = string.Empty;
        public DateTime PublishTime { get; set; }
        public DateTime CreatedTime { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")); // Test = Success :)
        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
        public List<ImageDb>? ImageDbs { get; set; }
    }
}