using AutoMapper;
using Microsoft.EntityFrameworkCore;
using newsApi.Data;
using newsApi.Dtos;
using newsApi.Models;

namespace newsApi.Services.CalendarEventService
{
    public class CalendarEventService : ICalendarEventService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public CalendarEventService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<Guid>> CreateCalendarEvent(CalendarEventDto calendarEventDto)
        {
            var sp = new ServiceResponse<Guid>();
            var id = Guid.NewGuid();

            if (await Exist(calendarEventDto.Title))
            {
                sp.Success = false;
                sp.Message = "Event with the Tittle: - " + calendarEventDto.Title + " - Allready exists.";
                return sp;
            }

            var calendarEvent = _mapper.Map<CalendarEvent>(calendarEventDto);
            calendarEvent.Id = id;
            sp.Data = calendarEvent.Id;

            try
            {
                _context.Add(calendarEvent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                sp.Success = false;
                sp.Message = ex.Message;
            }
            return sp;
        }

        public async Task<ServiceResponse<List<CalendarEvent>>> GetAllEvents()
        {
            var sp = new ServiceResponse<List<CalendarEvent>>();
            try
            {
                sp.Data = await _context.CalendarEvents.ToListAsync();
                if (sp.Data.Count == 0)
                {
                    sp.Success = false;
                    sp.Message = "No Events found.";
                }
            }
            catch (Exception ex)
            {
                sp.Success = false;
                sp.Message = ex.Message;
            }

            return sp;
        }

        private async Task<bool> Exist(string title)
        {
            var ce = await _context.CalendarEvents.FirstOrDefaultAsync(ce => ce.Title == title);
            return ce is null ? false : true;
        }
    }
}