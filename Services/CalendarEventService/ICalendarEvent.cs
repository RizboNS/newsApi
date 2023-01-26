using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.CalendarEventService
{
    public interface ICalendarEvent
    {
        Task<ServiceResponse<Guid>> CreateCalendarEvent(CalendarEventDto calendarEventDto);
    }
}