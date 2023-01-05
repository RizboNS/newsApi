using newsApi.Models;

namespace newsApi.Dtos
{
    public class StoryResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Category Category { get; set; }
        public bool Publish { get; set; }
        public string HtmlData { get; set; } = string.Empty;
        public List<ImageDb>? ImageDbs { get; set; }
        public DateTime PublishTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}