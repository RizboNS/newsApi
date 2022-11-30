namespace newsApi.Dtos
{
    public class StoryCreatedDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string HtmlData { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")); // Test = Success :)
    }
}