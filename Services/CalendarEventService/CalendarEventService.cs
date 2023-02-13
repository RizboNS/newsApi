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

        public async Task<ServiceResponse<Guid>> Delete(Guid id)
        {
            var sp = new ServiceResponse<Guid>();
            try
            {
                var calendarEvent = await _context.CalendarEvents.FirstOrDefaultAsync(ce => ce.Id == id);
                if (calendarEvent is not null)
                {
                    _context.Remove(calendarEvent);
                    await _context.SaveChangesAsync();
                    sp.Data = id;
                }
                else
                {
                    sp.Success = false;
                    sp.Message = "Event with the Id: - " + id + " - does not exist.";
                }
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

        public async Task<ServiceResponse<List<CalendarEventResponseDto>>> GetByDates(string startDate, string endDate)
        {
            var sp = new ServiceResponse<List<CalendarEventResponseDto>>();

            try
            {
                var startDateTime = DateTime.Parse(startDate);
                var endDateTime = DateTime.Parse(endDate);
                var events = await _context.CalendarEvents.Where(ce => ce.DateAndTime.Date >= startDateTime && ce.DateAndTime.Date <= endDateTime).ToListAsync();
                var dates = Enumerable.Range(0, (endDateTime - startDateTime).Days + 1)
                      .Select(d => startDateTime.AddDays(d))
                      .Select(d => new DateTime(d.Year, d.Month, d.Day, 0, 0, 0))
                      .ToList();

                sp.Data = new List<CalendarEventResponseDto>();
                foreach (var date in dates)
                {
                    var tmpDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                    sp.Data.Add(new CalendarEventResponseDto
                    {
                        Date = date,
                        Events = events.Where(ce => ce.DateAndTime.Date == tmpDate).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                sp.Success = false;
                sp.Message = ex.Message;
            }
            return sp;
        }

        public async Task<ServiceResponse<CalendarEvent>> GetById(Guid id)
        {
            var sp = new ServiceResponse<CalendarEvent>();
            try
            {
                sp.Data = await _context.CalendarEvents.FirstOrDefaultAsync(ce => ce.Id == id);
                if (sp.Data is null)
                {
                    sp.Success = false;
                    sp.Message = "Event with the Id: - " + id + " - does not exist.";
                }
            }
            catch (Exception ex)
            {
                sp.Success = false;
                sp.Message = ex.Message;
            }
            return sp;
        }

        public async Task<ServiceResponse<CalendarEvent>> Update(Guid id, CalendarEvent calendarEvent)
        {
            var sp = new ServiceResponse<CalendarEvent>();
            try
            {
                var calendarEventFromDb = await _context.CalendarEvents.FirstOrDefaultAsync(ce => ce.Id == id);
                if (calendarEventFromDb is null)
                {
                    sp.Success = false;
                    sp.Message = "Calendar Event with id: " + calendarEvent.Id + " not found.";
                    return sp;
                }
                calendarEventFromDb.Title = calendarEvent.Title;
                calendarEventFromDb.Content = calendarEvent.Content;
                calendarEventFromDb.DateAndTime = calendarEvent.DateAndTime;
                calendarEventFromDb.Type = calendarEvent.Type;

                await _context.SaveChangesAsync();
                sp.Data = calendarEventFromDb;
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