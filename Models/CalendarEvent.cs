namespace newsApi.Models
{
    public class CalendarEvent
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string HtmlData { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime DateAndTime { get; set; }
    }
}