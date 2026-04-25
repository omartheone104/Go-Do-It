using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Ical;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace GoDoIt;

[JsonConverter(typeof(RepeatIntervalJsonConverter))]
public enum RepeatInterval
{
    None, Daily, Weekly, Monthly, Yearly
}

public static class RepeatIntervalExtensions
{
    public static RecurrencePattern? TryGetRecurrencePattern(this RepeatInterval repeatInterval) => repeatInterval switch
    {
        RepeatInterval.None => null,
        RepeatInterval.Daily => new RecurrencePattern(FrequencyType.Daily),
        RepeatInterval.Weekly => new RecurrencePattern(FrequencyType.Weekly),
        RepeatInterval.Monthly => new RecurrencePattern(FrequencyType.Monthly),
        RepeatInterval.Yearly => new RecurrencePattern(FrequencyType.Yearly),
        _ => throw new NotImplementedException(),
    };

    public static IEnumerable<RecurrencePattern> GetRecurrencePatterns(this RepeatInterval repeatInterval) => repeatInterval switch
    {
        RepeatInterval.None => [],
        RepeatInterval.Daily => [new RecurrencePattern(FrequencyType.Daily)],
        RepeatInterval.Weekly => [new RecurrencePattern(FrequencyType.Weekly)],
        RepeatInterval.Monthly => [new RecurrencePattern(FrequencyType.Monthly)],
        RepeatInterval.Yearly => [new RecurrencePattern(FrequencyType.Yearly)],
        _ => throw new NotImplementedException(),
    };
}
[JsonConverter(typeof(EventJsonConverter))]
public class Event
{
    private RecurringComponent calendarEvent;
    private Guid? parentId;
    private bool isComplete;
    public Guid CategoryId => Guid.Parse(calendarEvent.Categories.First());
    public Guid? ParentId => parentId;
    public bool IsSubtask => parentId != null;
    public Guid Id
    {
        get
        {
            if (Guid.TryParse(calendarEvent.Uid, out var id))
            {
                return id;
            }
            else
            {
                id = Guid.NewGuid();
                calendarEvent.Uid = id.ToString();
                return id;
            }
        }
        internal set => calendarEvent.Uid = value.ToString();
    }

    public bool IsComplete => isComplete;
    public bool IsRepeating => calendarEvent.RecurrenceRules.Any();
    public string Title => calendarEvent.Summary ?? string.Empty;
    public string Description => calendarEvent.Description ?? string.Empty;

    public RepeatInterval RepeatInterval
    {
        get
        {
            if (!calendarEvent.RecurrenceRules.Any())
            {
                return RepeatInterval.None;
            }
            return calendarEvent.RecurrenceRules.First().Frequency switch
            {
                FrequencyType.Daily => RepeatInterval.Daily,
                FrequencyType.Weekly => RepeatInterval.Weekly,
                FrequencyType.Monthly => RepeatInterval.Monthly,
                FrequencyType.Yearly => RepeatInterval.Yearly,
                _ => RepeatInterval.None
            };
        }
    }
    public DateTime DueDate => calendarEvent.DtStart?.Value ?? throw new NullReferenceException("DueDate unexpectedly null"); // should never be null as we always set a start date

    public IEnumerable<DateTime> Occurances => calendarEvent.GetOccurrences()
        .Select(o => o.Period.StartTime.Value);

    public Event(string Title, string Description, DateTime DueDate, Guid CategoryId, Guid? ParentId = null, bool IsComplete = false, RepeatInterval RepeatInterval = RepeatInterval.None)
    {
        parentId = ParentId;
        isComplete = IsComplete;


        calendarEvent = new CalendarEvent()
        {
            Uid = Guid.NewGuid().ToString(),
            Categories = [CategoryId.ToString()],
            Summary = Title,
            Description = Description,
            DtStart = new CalDateTime(DueDate, TimeZoneInfo.Local.Id),
            RecurrenceRules = [.. RepeatInterval.GetRecurrencePatterns()],
        };
    }
    private Event(CalendarEvent calendarEvent)
    {
        this.calendarEvent = calendarEvent;

        if (Guid.TryParse(calendarEvent.Properties.First(p => p.Name == "X-PARENT-ID").Value?.ToString(), out var tempParentId))
        {
            parentId = tempParentId;
        }
        else
        {
            parentId = null;
        }

        if (!bool.TryParse(calendarEvent.Properties.First(p => p.Name == "X-IS-COMPLETE").Value?.ToString(), out isComplete))
        {
            isComplete = false;
        }
    }

    public CalendarEvent AsCalendarEvent()
    {
        CalendarEvent eventCopy = calendarEvent.Copy<CalendarEvent>()!;
        eventCopy.AddProperty("X-IS-COMPLETE", isComplete.ToString().ToUpperInvariant());
        eventCopy.AddProperty("X-PARENT-ID", parentId?.ToString() ?? "NONE");
        return eventCopy;
    }
    public static Event FromCalendarEvent(CalendarEvent calendarEvent) => new(calendarEvent);

    public bool DueOn(DateOnly date)
    {
        if (!IsRepeating) return calendarEvent.Start?.Date == date;

        var start = DateOnly.FromDateTime(DueDate);
        if (date < start) return false;

        return RepeatInterval switch
        {
            RepeatInterval.Daily => true,
            RepeatInterval.Weekly => (date.DayNumber - start.DayNumber) % 7 == 0,
            RepeatInterval.Monthly => date.Day == start.Day,
            RepeatInterval.Yearly => date.Day == start.Day && date.Month == start.Month,
            _ => false 
        };
    }

    public bool DueOn(DateTime date) => DueOn(DateOnly.FromDateTime(date));
    public bool DueToday() => !IsComplete && DueOn(DateTime.Today); 

    public DateTime? NextOccurrenceFrom(DateTime date) => DueDate.CompareTo(date) switch
    {
        _ when IsComplete => null,
        >= 0 => DueDate,
        < 0 when IsRepeating => Occurances.First(d => d >= date),
        _ => null,
    };

    public DateTime? NextOccurrence() => NextOccurrenceFrom(DateTime.Today);

    // override object.Equals
    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not Event other)
        {
            return false;
        }

        return Id == other.Id &&
        ParentId == other.ParentId &&
        IsComplete == other.IsComplete &&
        RepeatInterval == other.RepeatInterval &&
        Title == other.Title &&
        Description == other.Description &&
        DueDate == other.DueDate;
    }

    // override object.GetHashCode
    public override int GetHashCode() => HashCode.Combine(Id, ParentId, IsComplete, RepeatInterval, Title, Description, DueDate);
}
