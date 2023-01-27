using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.CalendarEventService
{
    public interface ICalendarEventService
    {
        Task<ServiceResponse<Guid>> CreateCalendarEvent(CalendarEventDto calendarEventDto);

        Task<ServiceResponse<List<CalendarEvent>>> GetAllEvents();
    }
}