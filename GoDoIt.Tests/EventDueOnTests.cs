using NUnit.Framework;
using System;
using GoDoIt;

namespace GoDoIt.Tests;

[TestFixture]
public class EventDueOnTests
{
    [Test]
    public void DueOn_DateOnly_ReturnsTrueWhenMatches()
    {
        var date = new DateTime(2026, 6, 15);
        var @event = new Event("Test", "Test", date, new Guid());
        Assert.That(@event.DueOn(DateOnly.FromDateTime(date)), Is.True);
    }

    [Test]
    public void DueOn_DateOnly_ReturnsFalseWhenDifferent()
    {
        var date = new DateTime(2026, 6, 15);
        var @event = new Event("Test", "Test", date, new Guid());
        Assert.That(@event.DueOn(DateOnly.FromDateTime(date.AddDays(1))), Is.False);
    }

    [Test]
    public void DueOn_DateTime_ReturnsTrueWhenMatches()
    {
        var date = new DateTime(2026, 6, 15);
        var @event = new Event("Test", "Test", date, new Guid());
        Assert.That(@event.DueOn(date), Is.True);
    }
}
