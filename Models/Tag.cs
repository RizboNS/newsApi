﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace newsApi.Models
{
    public class Tag
    {
        [Key, Column(Order = 0), Required, MinLength(2), MaxLength(50)]
        public string TagName { get; set; } = string.Empty;

        public string TagValue { get; set; } = string.Empty;

        public ICollection<StoryTag> StoryTags { get; set; } = new List<StoryTag>();
    }
}