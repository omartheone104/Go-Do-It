using NUnit.Framework;
using System;
using GoDoIt;

namespace GoDoIt.Tests;

[TestFixture]
public class EventConstructorTests
{
    [Test]
    public void Constructor_TitleIsPreserved()
    {
        var @event = new Event(
            Title: "My Task",
            Description: "Test",
            DueDate: DateTime.Today,
            CategoryId: new Guid()
        );
        Assert.That(@event.Title, Is.EqualTo("My Task"));
    }

    [Test]
    public void Constructor_DescriptionIsPreserved()
    {
        var @event = new Event(
            Title: "Test",
            Description: "My Description",
            DueDate: DateTime.Today,
            CategoryId: new Guid()
        );
        Assert.That(@event.Description, Is.EqualTo("My Description"));
    }

    [Test]
    public void Constructor_ParentIdIsPreserved()
    {
        var parentId = Guid.NewGuid();
        var @event = new Event(
            Title: "Test",
            Description: "Test",
            DueDate: DateTime.Today,
            CategoryId: new Guid(),
            ParentId: parentId
        );
        Assert.That(@event.ParentId, Is.EqualTo(parentId));
    }

    [Test]
    public void Constructor_DefaultParentIsNull()
    {
        var @event = new Event(
            Title: "Test Task",
            Description: "Test",
            DueDate: DateTime.Today,
            CategoryId: new Guid(),
            IsComplete: false,
            RepeatInterval: RepeatInterval.None
        );
        Assert.That(@event.ParentId, Is.Null);
    }

    [Test]
    public void Constructor_IsCompleteTrue_WhenPassedTrue()
    {
        var @event = new Event(
            Title: "Test",
            Description: "Test",
            DueDate: DateTime.Today,
            CategoryId: new Guid(),
            IsComplete: true
        );
        Assert.That(@event.IsComplete, Is.True);
    }

    [Test]
    public void Constructor_DefaultIsCompleteFalse()
    {
        var @event = new Event(
            Title: "Test Task",
            Description: "Test",
            DueDate: DateTime.Today,
            CategoryId: new Guid(),
            ParentId: null,
            RepeatInterval: RepeatInterval.None
        );
        Assert.That(@event.IsComplete, Is.False);
    }

    [Test]
    public void Constructor_IsRepeating_WhenRepeatIntervalSet()
    {
        var @event = new Event(
            Title: "Test",
            Description: "Test",
            DueDate: DateTime.Today,
            CategoryId: new Guid(),
            RepeatInterval: RepeatInterval.Weekly
        );
        Assert.That(@event.IsRepeating, Is.True);
    }

    [Test]
    public void Constructor_DefaultIsRepeatingFalse()
    {
        var @event = new Event(
            Title: "Test Task",
            Description: "Test",
            DueDate: DateTime.Today,
            CategoryId: new Guid(),
            ParentId: null,
            IsComplete: false
        );
        Assert.That(@event.IsRepeating, Is.False);
    }

    [Test]
    public void Constructor_TwoEvents_HaveDifferentIds()
    {
        var ev1 = new Event("A", "Test", DateTime.Today, new Guid());
        var ev2 = new Event("B", "Test", DateTime.Today, new Guid());
        Assert.That(ev1.Id, Is.Not.EqualTo(ev2.Id));
    }
}
