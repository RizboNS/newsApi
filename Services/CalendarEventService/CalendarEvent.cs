using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.CalendarEventService
{
    public class CalendarEvent : ICalendarEvent
    {
        public async Task<ServiceResponse<Guid>> CreateCalendarEvent(CalendarEventDto calendarEventDto)
        {
            var sp = new ServiceResponse<Guid>();
            var guidTest = Guid.NewGuid();
            sp.Data = guidTest;

            return sp;
        }
    }
}