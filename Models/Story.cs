﻿namespace newsApi.Models
{
    public class Story
    {
        public Guid Id { get; set; }
        public string TitleId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Category Category { get; set; }
        public string HtmlData { get; set; } = string.Empty;
        public DateTime PublishTime { get; set; }
        public bool Publish { get; set; }
        public DateTime CreatedTime { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
        public DateTime UpdateTime { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
        public List<ImageDb> ImageDbs { get; set; } = new List<ImageDb>();
        public ICollection<StoryTag> StoryTags { get; set; } = new List<StoryTag>();
    }
}