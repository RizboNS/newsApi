using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newsApi.Dtos;
using newsApi.Models;
using newsApi.Services.CalendarEventService;

namespace newsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarEventController : ControllerBase
    {
        private readonly ICalendarEventService _calendarEvent;

        public CalendarEventController(ICalendarEventService calendarEvent)
        {
            _calendarEvent = calendarEvent;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCalendarEvent([FromBody] CalendarEventDto calendarEventDto)
        {
            var serviceResponse = await _calendarEvent.CreateCalendarEvent(calendarEventDto);
            if (serviceResponse.Success == false)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }
    }
}