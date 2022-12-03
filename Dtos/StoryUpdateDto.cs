﻿using newsApi.Models;

namespace newsApi.Dtos
{
    public class StoryUpdateDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Category Category { get; set; }
        public string HtmlData { get; set; } = string.Empty;
        public DateTime PublishTime { get; set; }
    }
}