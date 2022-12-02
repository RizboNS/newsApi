namespace newsApi.Models
{
    public class Story
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HtmlData { get; set; } = string.Empty;
        public DateTime PublishTime { get; set; }
        public DateTime CreatedTime { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
        public DateTime UpdateTime { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
        public List<ImageDb>? ImageDbs { get; set; }
    }
}