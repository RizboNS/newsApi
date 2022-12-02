﻿using newsApi.Models;

namespace newsApi.Dtos
{
    public class StoryResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HtmlData { get; set; } = string.Empty;
        public DateTime PublishTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public List<ImageDb>? ImageDbs { get; set; }
    }
}