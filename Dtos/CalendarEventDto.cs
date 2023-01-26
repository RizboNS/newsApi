namespace newsApi.Dtos
{
    public class CalendarEventDto
    {
        public Guid? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime DateAndTime { get; set; }
    }
}