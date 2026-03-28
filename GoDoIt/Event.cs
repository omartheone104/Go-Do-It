using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ical;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
namespace GoDoIt;


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

public class Event
{
    private CalendarEvent calendarEvent;
    // private readonly Guid id = new();
    public Guid CategoryId => Guid.Parse(calendarEvent.Categories.First());
    private Guid? parentId;
    public Guid? ParentId => parentId;
    // private bool isComplete;
    public Guid Id => Guid.Parse(calendarEvent.Uid ?? throw new NullReferenceException());

    private bool isComplete;
    public bool IsComplete => isComplete;

    public bool IsRepeating => calendarEvent.RecurrenceRules.Any();

    public DateTime DueDate => calendarEvent.DtStart?.Value.ToLocalTime() ?? throw new NullReferenceException(); // should never be null as we always set a start date

    public IEnumerable<DateTime> Occurances => calendarEvent.GetOccurrences().Select(o => o.Period.StartTime.Value.ToLocalTime());

    public Event(string Title, string Description, DateTime DueDate, Guid CategoryId, Guid? ParentId = null, bool IsComplete = false, RepeatInterval RepeatInterval = RepeatInterval.None)
    {
        parentId = ParentId;
        isComplete = IsComplete;

        calendarEvent = new()
        {
            Uid = new Guid().ToString(),
            Categories = [CategoryId.ToString()],
            Summary = Title,
            Description = Description,
            DtStart = new CalDateTime(DueDate.ToUniversalTime()),
            RecurrenceRules = [.. RepeatInterval.GetRecurrencePatterns()]
        };
    }

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

    // public DateTime? NextOccurrenceFrom(DateTime date) => NextOccurrenceFrom(DateOnly.FromDateTime(date));
    public DateTime? NextOccurrence() => NextOccurrenceFrom(DateTime.Today);
}