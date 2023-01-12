using System.ComponentModel.DataAnnotations.Schema;

namespace newsApi.Models
{
    public class StoryTag
    {
        [ForeignKey("Story")]
        public Guid StoryId { get; set; }

        public Story Story { get; set; } = null!;

        [ForeignKey("Tag")]
        public string TagName { get; set; } = string.Empty;

        public Tag Tag { get; set; } = null!;
    }
}