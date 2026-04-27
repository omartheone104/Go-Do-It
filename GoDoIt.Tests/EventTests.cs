using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using GoDoIt;
using Ical.Net.Serialization;
using Ical.Net.CalendarComponents;

namespace GoDoIt.Tests;

[TestFixture]
public class EventTests
{
    private static readonly Event[] testEvents =
    [
        new Event("Test Task 1", "Test Description 1", DateTime.Today, Guid.NewGuid(), null, true, RepeatInterval.None),
        new Event("Test Task 2", "Test Description 2", DateTime.Today, Guid.NewGuid(), null, true, RepeatInterval.Daily),
        new Event("Test Task 3", "Test Description 3", DateTime.Today, Guid.NewGuid(), null, true, RepeatInterval.Weekly),
        new Event("Test Task 4", "Test Description 4", DateTime.Today, Guid.NewGuid(), null, true, RepeatInterval.Monthly),
        new Event("Test Task 5", "Test Description 5", DateTime.Today, Guid.NewGuid(), null, true, RepeatInterval.Yearly),
        new Event("Test Task 6", "Test Description 6", DateTime.Today, Guid.NewGuid(), null, false, RepeatInterval.None),
        new Event("Test Task 7", "Test Description 7", DateTime.Today, Guid.NewGuid(), null, false, RepeatInterval.Daily),
        new Event("Test Task 8", "Test Description 8", DateTime.Today, Guid.NewGuid(), null, false, RepeatInterval.Weekly),
        new Event("Test Task 9", "Test Description 9", DateTime.Today, Guid.NewGuid(), null, false, RepeatInterval.Monthly),
        new Event("Test Task 10", "Test Description 10", DateTime.Today, Guid.NewGuid(), null, false, RepeatInterval.Yearly),
        new Event("Test Task 11", "Test Description 11", DateTime.Today, Guid.NewGuid(), Guid.NewGuid(), true, RepeatInterval.None),
        new Event("Test Task 12", "Test Description 12", DateTime.Today, Guid.NewGuid(), Guid.NewGuid(), true, RepeatInterval.Daily),
        new Event("Test Task 13", "Test Description 13", DateTime.Today, Guid.NewGuid(), Guid.NewGuid(), true, RepeatInterval.Weekly),
        new Event("Test Task 14", "Test Description 14", DateTime.Today, Guid.NewGuid(), Guid.NewGuid(), true, RepeatInterval.Monthly),
        new Event("Test Task 15", "Test Description 15", DateTime.Today, Guid.NewGuid(), Guid.NewGuid(), true, RepeatInterval.Yearly),
        new Event("Test Task 16", "Test Description 16", DateTime.Today, Guid.NewGuid(), Guid.NewGuid(), false, RepeatInterval.None),
        new Event("Test Task 17", "Test Description 17", DateTime.Today, Guid.NewGuid(), Guid.NewGuid(), false, RepeatInterval.Daily),
        new Event("Test Task 18", "Test Description 18", DateTime.Today, Guid.NewGuid(), Guid.NewGuid(), false, RepeatInterval.Weekly),
        new Event("Test Task 19", "Test Description 19", DateTime.Today, Guid.NewGuid(), Guid.NewGuid(), false, RepeatInterval.Monthly),
        new Event("Test Task 20", "Test Description 20", DateTime.Today, Guid.NewGuid(), Guid.NewGuid(), false, RepeatInterval.Yearly),
        new Event("Test Task 21", "Test Description 21", DateTimeOffset.FromUnixTimeSeconds(253222763).UtcDateTime,  Guid.NewGuid(), null, true, RepeatInterval.None),
        new Event("Test Task 22", "Test Description 22", DateTimeOffset.FromUnixTimeSeconds(1881907738).UtcDateTime, Guid.NewGuid(), null, true, RepeatInterval.Daily),
        new Event("Test Task 23", "Test Description 23", DateTimeOffset.FromUnixTimeSeconds(3503226702).UtcDateTime, Guid.NewGuid(), null, true, RepeatInterval.Weekly),
        new Event("Test Task 24", "Test Description 24", DateTimeOffset.FromUnixTimeSeconds(3331792913).UtcDateTime, Guid.NewGuid(), null, true, RepeatInterval.Monthly),
        new Event("Test Task 25", "Test Description 25", DateTimeOffset.FromUnixTimeSeconds(366397947).UtcDateTime,  Guid.NewGuid(), null, true, RepeatInterval.Yearly),
        new Event("Test Task 26", "Test Description 26", DateTimeOffset.FromUnixTimeSeconds(838917024).UtcDateTime,  Guid.NewGuid(), null, false, RepeatInterval.None),
        new Event("Test Task 27", "Test Description 27", DateTimeOffset.FromUnixTimeSeconds(2944912136).UtcDateTime, Guid.NewGuid(), null, false, RepeatInterval.Daily),
        new Event("Test Task 28", "Test Description 28", DateTimeOffset.FromUnixTimeSeconds(782037370).UtcDateTime,  Guid.NewGuid(), null, false, RepeatInterval.Weekly),
        new Event("Test Task 29", "Test Description 29", DateTimeOffset.FromUnixTimeSeconds(931164482).UtcDateTime,  Guid.NewGuid(), null, false, RepeatInterval.Monthly),
        new Event("Test Task 30", "Test Description 30", DateTimeOffset.FromUnixTimeSeconds(3176260235).UtcDateTime, Guid.NewGuid(), null, false, RepeatInterval.Yearly),
        new Event("Test Task 31", "Test Description 31", DateTimeOffset.FromUnixTimeSeconds(1863100829).UtcDateTime, Guid.NewGuid(), Guid.NewGuid(), true, RepeatInterval.None),
        new Event("Test Task 32", "Test Description 32", DateTimeOffset.FromUnixTimeSeconds(815242392).UtcDateTime,  Guid.NewGuid(), Guid.NewGuid(), true, RepeatInterval.Daily),
        new Event("Test Task 33", "Test Description 33", DateTimeOffset.FromUnixTimeSeconds(3935534374).UtcDateTime, Guid.NewGuid(), Guid.NewGuid(), true, RepeatInterval.Weekly),
        new Event("Test Task 34", "Test Description 34", DateTimeOffset.FromUnixTimeSeconds(1648655535).UtcDateTime, Guid.NewGuid(), Guid.NewGuid(), true, RepeatInterval.Monthly),
        new Event("Test Task 35", "Test Description 35", DateTimeOffset.FromUnixTimeSeconds(386828443).UtcDateTime,  Guid.NewGuid(), Guid.NewGuid(), true, RepeatInterval.Yearly),
        new Event("Test Task 36", "Test Description 36", DateTimeOffset.FromUnixTimeSeconds(381891601).UtcDateTime,  Guid.NewGuid(), Guid.NewGuid(), false,RepeatInterval.None),
        new Event("Test Task 37", "Test Description 37", DateTimeOffset.FromUnixTimeSeconds(4264577284).UtcDateTime, Guid.NewGuid(), Guid.NewGuid(), false, RepeatInterval.Daily),
        new Event("Test Task 38", "Test Description 38", DateTimeOffset.FromUnixTimeSeconds(315763967).UtcDateTime,  Guid.NewGuid(), Guid.NewGuid(), false, RepeatInterval.Weekly),
        new Event("Test Task 39", "Test Description 39", DateTimeOffset.FromUnixTimeSeconds(454790657).UtcDateTime,  Guid.NewGuid(), Guid.NewGuid(), false, RepeatInterval.Monthly),
        new Event("Test Task 40", "Test Description 40", DateTimeOffset.FromUnixTimeSeconds(1813843450).UtcDateTime, Guid.NewGuid(), Guid.NewGuid(), false, RepeatInterval.Yearly),
    ];

    [Test]
    public void DueToday_WhenDateIsToday_ReturnsTrue()
    {
        var ev = new Event("Today", "Today Test Event", DateTime.Today, new Guid(),
            null, false, RepeatInterval.None);
        Assert.That(ev.DueToday(), Is.True);
    }

    [Test]
    public void DueToday_WhenDateIsFuture_ReturnsFalse()
    {
        var ev = new Event("Future", "Future Test Event", DateTime.Today.AddDays(10), new Guid(),
            null, false, RepeatInterval.None);
        Assert.That(ev.DueToday(), Is.False);
    }

    [Test]
    public void DueToday_WhenDateIsPast_ReturnsFalse()
    {
        var ev = new Event("Past", "Past Test Event", DateTime.Today.AddDays(-10), new Guid(),
            null, false, RepeatInterval.None);
        Assert.That(ev.DueToday(), Is.False);
    }

    [Test]
    public void CompletedEvent_IsNotDueToday()
    {
        var ev = new Event("Completed Task", "Test", DateTime.Today, new Guid(),
            null, true, RepeatInterval.None);
        Assert.That(ev.DueToday(), Is.False);
    }

    [Test]
    public void Event_CategoryUUIDPreserved()
    {
        var catId = new Guid();
        var @event = new Event("Test Event", "Test", DateTime.Today, catId,
            null, false, RepeatInterval.None);
        Assert.That(@event.CategoryId, Is.EqualTo(catId));
    }

    [Test]
    public void NextOccurrenceFrom_DueAfterCheckedDate_ReturnsFirstOccurrence()
    {
        var dueDate = new DateTime(2025, 1, 1);
        var @event = new Event("Test Task", "Test", dueDate, new Guid(),
            null, false, RepeatInterval.Daily);
        Assert.That(@event.NextOccurrenceFrom(dueDate.AddDays(-10)), Is.EqualTo(dueDate));
    }

    [Test]
    public void NextOccurrenceFrom_DueBeforeCheckedDate_WithOccurrenceOnCheckedDate_ReturnsNextOccurrence()
    {
        var dueDate = new DateTime(2025, 1, 1);
        var @event = new Event("Test Task", "Test", dueDate, new Guid(),
            null, false, RepeatInterval.Daily);
        Assert.That(@event.NextOccurrenceFrom(dueDate.AddDays(10)), Is.EqualTo(dueDate.AddDays(10)));
    }

    [Test]
    public void NextOccurrenceFrom_DueBeforeCheckedDate_WithoutOccurrenceOnCheckedDate_ReturnsNextOccurrence()
    {
        var dueDate = new DateTime(2025, 1, 1);
        var @event = new Event("Test Task", "Test", dueDate, new Guid(),
            null, false, RepeatInterval.Monthly);
        Assert.That(@event.NextOccurrenceFrom(new DateTime(2025, 1, 2)), Is.EqualTo(new DateTime(2025, 2, 1)));
    }

    [Test]
    public void NextOccurrenceFrom_WeeklyRepeat_ReturnsCorrectDate()
    {
        var dueDate = new DateTime(2025, 1, 1); // Wednesday
        var @event = new Event("Test", "Test", dueDate, new Guid(),
            null, false, RepeatInterval.Weekly);
        Assert.That(@event.NextOccurrenceFrom(new DateTime(2025, 1, 3)), Is.EqualTo(new DateTime(2025, 1, 8)));
    }

    [Test]
    public void NextOccurrenceFrom_WhenEventIsComplete_ReturnsNull()
    {
        var dueDate = new DateTime(2025, 1, 11);
        var @event = new Event("Test Task", "Test", dueDate, new Guid(),
            null, true, RepeatInterval.Daily);
        Assert.That(@event.NextOccurrenceFrom(dueDate.AddDays(10)), Is.Null);
    }

    [Test]
    public void NextOccurrenceFrom_WhenNoRepeat_AndCheckedDateAfterDueDate_ReturnsNull()
    {
        var dueDate = new DateTime(2025, 1, 11);
        var @event = new Event("Test Task", "Test", dueDate, new Guid(),
            null, false, RepeatInterval.None);
        Assert.That(@event.NextOccurrenceFrom(dueDate.AddDays(10)), Is.Null);
    }

    [Test]
    public void NextOccurrenceFrom_WhenNoRepeat_AndCheckedDateBeforeDueDate_ReturnsDueDate()
    {
        var dueDate = new DateTime(2025, 1, 11);
        var @event = new Event("Test Task", "Test", dueDate, new Guid(),
            null, false, RepeatInterval.None);
        Assert.That(@event.NextOccurrenceFrom(dueDate.AddDays(-10)), Is.EqualTo(dueDate));
    }

    [Test]
    public void NextOccurrence_WhenDueToday_ReturnsToday()
    {
        var @event = new Event("Test Task", "Test", DateTime.Today, new Guid(),
            null, false, RepeatInterval.None);
        Assert.That(@event.NextOccurrence(), Is.EqualTo(DateTime.Today));
    }

    [Test]
    public void NextOccurrence_WhenDueInFuture_ReturnsDueDate()
    {
        var dueDate = DateTime.Today.AddDays(10);
        var @event = new Event("Test Task", "Test", dueDate, new Guid(),
            null, false, RepeatInterval.None);
        Assert.That(@event.NextOccurrence(), Is.EqualTo(dueDate));
    }

    [Test]
    public void NextOccurrence_WhenDueInPast_NoRepeat_ReturnsNull()
    {
        var @event = new Event("Test Task", "Test", DateTime.Today.AddDays(-10), new Guid(),
            null, false, RepeatInterval.None);
        Assert.That(@event.NextOccurrence(), Is.Null);
    }

    [Test]
    public void NextOccurrence_WhenDueInPast_DailyRepeat_ReturnsToday()
    {
        var @event = new Event("Test Task", "Test", DateTime.Today.AddDays(-10), new Guid(),
            null, false, RepeatInterval.Daily);
        Assert.That(@event.NextOccurrence(), Is.EqualTo(DateTime.Today));
    }

    [Test]
    public void NextOccurrence_WhenDueInPast_YearlyRepeat_ReturnsNextYear()
    {
        var dueDate = DateTime.Today.AddDays(-10);
        var @event = new Event("Test Task", "Test", dueDate, new Guid(),
            null, false, RepeatInterval.Yearly);
        Assert.That(@event.NextOccurrence(), Is.EqualTo(dueDate.AddYears(1)));
    }

    [Test]
    public void NextOccurrence_WhenDueInPast_AndComplete_ReturnsNull()
    {
        var @event = new Event("Test Task", "Test", DateTime.Today.AddDays(-10), new Guid(),
            null, true, RepeatInterval.None);
        Assert.That(@event.NextOccurrence(), Is.Null);
    }

    [Test]
    public void NextOccurrence_WhenDueToday_AndComplete_ReturnsNull()
    {
        var @event = new Event("Test Task", "Test", DateTime.Today, new Guid(),
            null, true, RepeatInterval.None);
        Assert.That(@event.NextOccurrence(), Is.Null);
    }

    [Test]
    public void NextOccurrence_WhenDueInFuture_AndComplete_ReturnsNull()
    {
        var @event = new Event("Test Task", "Test", DateTime.Today.AddDays(10), new Guid(),
            null, true, RepeatInterval.None);
        Assert.That(@event.NextOccurrence(), Is.Null);
    }

    [Test]
    public void Event_YearlyRepeat_OnFebruary29_OnlyEveryFourYears()
    {
        var @event = new Event("Test Task", "Test", new DateTime(2024, 2, 29), new Guid(),
            null, false, RepeatInterval.Yearly);
        Assert.That(@event.NextOccurrenceFrom(new DateTime(2026, 2, 1)), Is.EqualTo(new DateTime(2028, 2, 29)));
    }

    [Test]
    public void Occurrences_CountOne_WhenNonRepeat()
    {
        var @event = new Event("Test Task", "Test", DateTime.Today, new Guid(),
            null, false, RepeatInterval.None);
        Assert.That(@event.Occurances.Count(), Is.EqualTo(1));
    }

    [Test]
    public void FirstOccurrence_IsDueDate_WhenNoRepeat()
    {
        var @event = new Event("Test Task", "Test", DateTime.Today, new Guid(),
            null, false, RepeatInterval.None);
        Assert.That(@event.Occurances.First(), Is.EqualTo(DateTime.Today));
    }

    [Test]
    public void FirstOccurrence_IsDueDate_WhenRepeat()
    {
        var @event = new Event("Test Task", "Test", DateTime.Today, new Guid(),
            null, false, RepeatInterval.Daily);
        Assert.That(@event.Occurances.First(), Is.EqualTo(DateTime.Today));
    }

    [Test]
    public void IdenticalEvents_HaveDifferentIds()
    {
        var event1 = new Event("Test Task", "Test", DateTime.Today, new Guid(),
            null, false, RepeatInterval.Daily);
        var event2 = new Event("Test Task", "Test", DateTime.Today, new Guid(),
            null, false, RepeatInterval.Daily);
        Assert.That(event1.Id, Is.Not.EqualTo(event2.Id));
    }

    [Test]
    public void EventsWithSameDetails_AreNotEqual()
    {
        var event1 = new Event("Test Task", "Test", DateTime.Today, new Guid(),
            null, false, RepeatInterval.Daily);
        var event2 = new Event("Test Task", "Test", DateTime.Today, new Guid(),
            null, false, RepeatInterval.Daily);
        Assert.That(event1, Is.Not.EqualTo(event2));
        Assert.That(event1.GetHashCode(), Is.Not.EqualTo(event2.GetHashCode()));
    }

    [Test]
    public void IdenticalEvents_HaveEqualProperties()
    {
        var catId = new Guid();
        var event1 = new Event("Test Task", "Test", DateTime.Today, catId,
            null, false, RepeatInterval.Daily);
        var event2 = new Event("Test Task", "Test", DateTime.Today, catId,
            null, false, RepeatInterval.Daily);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(event1.Title, Is.EqualTo(event2.Title));
            Assert.That(event1.Description, Is.EqualTo(event2.Description));
            Assert.That(event1.DueDate, Is.EqualTo(event2.DueDate));
            Assert.That(event1.CategoryId, Is.EqualTo(event2.CategoryId));
            Assert.That(event1.ParentId, Is.EqualTo(event2.ParentId));
            Assert.That(event1.IsComplete, Is.EqualTo(event2.IsComplete));
            Assert.That(event1.RepeatInterval, Is.EqualTo(event2.RepeatInterval));
        }
    }

    [Test]
    public void ToCalendarEvent_RoundTrip_ReturnsIdenticalObject(
        [ValueSource(nameof(testEvents))] Event event1)
    {
        var event2 = Event.FromCalendarEvent(event1.AsCalendarEvent());
        Assert.That(event1, Is.EqualTo(event2));
    }

    [Test]
    public void ToCalendarEvent_RoundTripToString_ReturnsIdenticalObject(
        [ValueSource(nameof(testEvents))] Event event1)
    {
        var calString = new CalendarSerializer().SerializeToString(event1.AsCalendarEvent());
        Assert.That(calString, Is.Not.Null);

        var calEvent2 = Ical.Net.Calendar.Load<CalendarEvent>(calString).FirstOrDefault();
        Assert.That(calEvent2, Is.Not.Null);

        var event2 = Event.FromCalendarEvent(calEvent2);
        Assert.That(event1, Is.EqualTo(event2));
    }
}
