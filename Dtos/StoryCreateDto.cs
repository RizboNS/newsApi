﻿using newsApi.Models;

namespace newsApi.Dtos
{
    public class StoryCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string TitleId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Category Category { get; set; }
        public string HtmlData { get; set; } = string.Empty;
        public DateTime PublishTime { get; set; }
        public bool Publish { get; set; }
        public List<Tag>? Tags { get; set; }
    }
}