using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.CalendarEventService
{
    public interface ICalendarEventService
    {
        Task<ServiceResponse<Guid>> CreateCalendarEvent(CalendarEventDto calendarEventDto);

        Task<ServiceResponse<List<CalendarEvent>>> GetAllEvents();

        Task<ServiceResponse<List<CalendarEventResponseDto>>> GetByDates(string startDate, string endDate);

        Task<ServiceResponse<Guid>> Delete(Guid id);

        Task<ServiceResponse<CalendarEvent>> Update(Guid id, CalendarEvent calendarEvent);

        Task<ServiceResponse<CalendarEvent>> GetById(Guid id);
    }
}