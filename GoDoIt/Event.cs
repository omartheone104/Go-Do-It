using System;
using System.Collections.Generic;
using System.Linq;
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
    private Guid? parentId;
    private bool isComplete;

    public Guid CategoryId => Guid.Parse(calendarEvent.Categories.First());
    public Guid? ParentId => parentId;
    public Guid Id => Guid.Parse(calendarEvent.Uid ?? throw new NullReferenceException());

    public bool IsComplete => isComplete;
    public bool IsRepeating => calendarEvent.RecurrenceRules.Any();
    public string Title => calendarEvent.Summary ?? string.Empty;
    public string Description => calendarEvent.Description ?? string.Empty;
    public RepeatInterval RepeatInterval { get; private set; } = RepeatInterval.None;

    public DateTime DueDate => calendarEvent.DtStart?.Value ?? throw new NullReferenceException();

    public IEnumerable<DateTime> Occurances => calendarEvent.GetOccurrences()
        .Select(o => o.Period.StartTime.Value);

    public Event(string Title, string Description, DateTime DueDate, Guid CategoryId, Guid? ParentId = null, bool IsComplete = false, RepeatInterval RepeatInterval = RepeatInterval.None)
    {
        parentId = ParentId;
        isComplete = IsComplete;
        this.RepeatInterval = RepeatInterval;

        calendarEvent = new()
        {
            Uid = Guid.NewGuid().ToString(),
            Categories = [CategoryId.ToString()],
            Summary = Title,
            Description = Description,
            DtStart = new CalDateTime(DateTime.SpecifyKind(DueDate, DateTimeKind.Unspecified)), 
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

    public DateTime? NextOccurrence() => NextOccurrenceFrom(DateTime.Today);
}
