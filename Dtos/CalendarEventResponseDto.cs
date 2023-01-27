using newsApi.Models;

namespace newsApi.Dtos
{
    public class CalendarEventResponseDto
    {
        public DateTime Date { get; set; }
        public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
    }
}