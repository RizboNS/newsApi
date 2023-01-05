using newsApi.Models;

namespace newsApi.Dtos
{
    public class StoryCreatedDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Category Category { get; set; }
        public string HtmlData { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public DateTime PublishTime { get; set; }
        public bool Publish { get; set; }
    }
}