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
    private CalendarEvent calendarEvent;
    private Guid? parentId;
    private bool isComplete;

    public Guid CategoryId => Guid.Parse(calendarEvent.Categories.First());
    public Guid? ParentId => parentId;
    // private bool isComplete;
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
    public DateTime DueDate => calendarEvent.DtStart?.Value.ToLocalTime() ?? throw new NullReferenceException("DueDate unexpectedly null"); // should never be null as we always set a start date

    public IEnumerable<DateTime> Occurances => calendarEvent.GetOccurrences()
        .Select(o => o.Period.StartTime.Value.ToLocalTime());

    public Event(string Title, string Description, DateTime DueDate, Guid CategoryId, Guid? ParentId = null, bool IsComplete = false, RepeatInterval RepeatInterval = RepeatInterval.None)
    {
        parentId = ParentId;
        isComplete = IsComplete;


        calendarEvent = new()
        {
            Uid = Guid.NewGuid().ToString(),
            Categories = [CategoryId.ToString()],
            Summary = Title,
            Description = Description,
            DtStart = new CalDateTime(DueDate.ToUniversalTime()),
            RecurrenceRules = [.. RepeatInterval.GetRecurrencePatterns()],
        };
    }
    private Event(CalendarEvent calendarEvent)
    {
        this.calendarEvent = calendarEvent;
        // var parentIdString = calendarEvent.Properties.First(p => p.Name == "X-PARENT-ID").Value;
        if (calendarEvent.Properties.First(p => p.Name == "X-PARENT-ID").Value is string parentIdString)
        {
            try
            {
                parentId = Guid.Parse(parentIdString);
            }
            catch (FormatException)
            {
                parentId = null;
            }
        }

        isComplete = calendarEvent.Properties.First(p => p.Name == "X-IS-COMPLETE").Value as bool? ?? false;
    }

    public CalendarEvent AsCalendarEvent()
    {
        CalendarEvent eventCopy = calendarEvent.Copy<CalendarEvent>()!;
        eventCopy.AddProperty(new CalendarProperty("X-IS-COMPLETE", isComplete));
        eventCopy.AddProperty(new CalendarProperty("X-PARENT-ID", parentId?.ToString() ?? "none"));
        return eventCopy;
    }
    public static Event FromCalendarEvent(CalendarEvent calendarEvent) => new(calendarEvent);

    public bool DueOn(DateOnly date) => !IsComplete && (calendarEvent.Start?.Date == date);
    public bool DueOn(DateTime date) => DueOn(DateOnly.FromDateTime(date));
    public bool DueToday() => DueOn(DateTime.Today);

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
