namespace newsApi.Dtos
{
    public class ImageSavedDto
    {
        public Guid Id { get; set; }
        public string LocationPath { get; set; } = string.Empty;
        public string LocationDomain { get; set; } = string.Empty;
        public Guid StoryId { get; set; }
    }
}