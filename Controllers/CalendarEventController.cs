using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newsApi.Models;

namespace newsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarEventController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateCalendarEvent([FromBody] CalendarEvent calendarEvent)
        {
            Console.WriteLine(calendarEvent.Title);
            return Ok();
        }
    }
}