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

        [HttpGet("ByDates")]
        public async Task<IActionResult> GetEventsByDates([FromQuery] string startDate, string endDate)
        {
            var serviceResponse = await _calendarEvent.GetByDates(startDate, endDate);
            if (serviceResponse.Success == false)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllEvents()
        {
            var serviceResponse = await _calendarEvent.GetAllEvents();
            if (serviceResponse.Success == false)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var serviceResponse = await _calendarEvent.Delete(id);
            if (serviceResponse.Success == false)
            {
                return BadRequest(serviceResponse);
            }
            return Ok(serviceResponse);
        }
    }
}