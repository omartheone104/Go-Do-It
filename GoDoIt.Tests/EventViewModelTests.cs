using NUnit.Framework;
using System;
using Avalonia.Media;
using GoDoIt;
using GoDoIt.ViewModels;

namespace GoDoIt.Tests;

[TestFixture]
public class EventViewModelTests
{
    [Test]
    public void EventViewModel_ColorFromMatchingCategory()
    {
        var cat = new Category("Work", Colors.LightPink);
        var ev = new Event("Test", "Test", DateTime.Today, cat.Id);
        var evm = new EventViewModel(ev, new[] { cat });
        Assert.That(evm.Color, Is.EqualTo(Colors.LightPink));
    }

    [Test]
    public void EventViewModel_ColorFallsBackToLightGray_WhenCategoryNotFound()
    {
        var ev = new Event("Test", "Test", DateTime.Today, Guid.NewGuid());
        var evm = new EventViewModel(ev, Array.Empty<Category>());
        Assert.That(evm.Color, Is.EqualTo(Colors.LightGray));
    }

    [Test]
    public void EventViewModel_TitleMatchesEvent()
    {
        var cat = new Category("Work", Colors.LightBlue);
        var ev = new Event("My Title", "Test", DateTime.Today, cat.Id);
        var evm = new EventViewModel(ev, new[] { cat });
        Assert.That(evm.Title, Is.EqualTo("My Title"));
    }

    [Test]
    public void EventViewModel_DescriptionMatchesEvent()
    {
        var cat = new Category("Work", Colors.LightBlue);
        var ev = new Event("Test", "My Description", DateTime.Today, cat.Id);
        var evm = new EventViewModel(ev, new[] { cat });
        Assert.That(evm.Description, Is.EqualTo("My Description"));
    }

    [Test]
    public void EventViewModel_DueDateMatchesEvent()
    {
        var date = new DateTime(2026, 5, 10);
        var cat = new Category("Work", Colors.LightBlue);
        var ev = new Event("Test", "Test", date, cat.Id);
        var evm = new EventViewModel(ev, new[] { cat });
        Assert.That(evm.DueDate, Is.EqualTo(date));
    }
}
