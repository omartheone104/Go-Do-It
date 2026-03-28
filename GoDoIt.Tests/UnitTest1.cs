using NUnit.Framework;
using NUnit.Framework.Legacy;
using Avalonia.Headless.NUnit;
using Avalonia.Controls;
using GoDoIt.Views;
using GoDoIt.ViewModels;
using System;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Ical.Net.DataTypes;


namespace GoDoIt.Tests
{
    [TestFixture]
    public class BasicTests
    {
        [Test]
        public void SaveAndLoad_String_Works()
        {
            using var stream = new MemoryStream();
            var data = Encoding.UTF8.GetBytes("Go Do It Test");
            stream.Write(data, 0, data.Length);

            stream.Position = 0;
            var loaded = new byte[stream.Length];
            stream.Read(loaded, 0, loaded.Length);

            string result = Encoding.UTF8.GetString(loaded);
            Assert.That(result, Is.EqualTo("Go Do It Test"));
        }

        [AvaloniaTest]
        public void Check_Calendar()
        {
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            // Show the window, as it's required to get layout processed:
            var date = new DateTime(2026, 3, 18, 00, 00, 00, 000);
            window.Show();
            Assert.That(window.Find<Avalonia.Controls.Calendar>("calendar"), Is.Not.Null);
            window.Find<Avalonia.Controls.Calendar>("calendar").SelectedDate = date;
            Assert.That(window.Find<TextBlock>("calendar_text"), Is.Not.Null);
            Assert.That(window.Find<TextBlock>("calendar_text").Text, Is.EqualTo(date.ToString()));
        }

    }

    [TestFixture]
    public class EventTests
    {
        [Test]
        public void DueToday_WhenDateIsToday_ReturnsTrue()
        {
            var TodayEvent = new Event(
                Title: "Today",
                Description: "Today Test Event",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(TodayEvent.DueToday(), Is.True);
        }

        [Test]
        public void DueToday_WhenDateIsFuture_ReturnsFalse()
        {
            var FutureEvent = new Event(
                Title: "Future",
                Description: "Future Test Event",
                DueDate: DateTime.Today.AddDays(10),
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(FutureEvent.DueToday(), Is.False);
        }

        [Test]
        public void DueToday_WhenDateIsPast_ReturnsFalse()
        {
            var PastEvent = new Event(
                Title: "Past",
                Description: "Past Test Event",
                DueDate: DateTime.Today.AddDays(-10),
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(PastEvent.DueToday(), Is.False);
        }
        [Test]
        public void CompletedEvent_IsNotDueToday()
        {
            var completedEvent = new Event(
                Title: "Completed Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: true,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(completedEvent.DueToday(), Is.False);
        }

        [Test]
        public void Event_CategoryUUIDPreserved()
        {
            var catId = new Guid();

            var @event = new Event(
                Title: "Test Event",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: catId,
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.CategoryId, Is.EqualTo(catId));
        }

        [Test]
        public void NextOccurrenceFrom_DueAfterCheckedDate_ReturnsFirstOccurrence()
        {
            var dueDate = new DateTime(2025, 1, 1);
            var checkedDate = dueDate.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.EqualTo(dueDate));
        }

        [Test]
        public void NextOccurrenceFrom_DueBeforeCheckedDate_WithOccuranceOnCheckedDate_ReturnsNextOccurance()
        {
            var dueDate = new DateTime(2025, 1, 1);
            var checkedDate = dueDate.AddDays(10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.EqualTo(dueDate.AddDays(10)));
        }
        [Test]
        public void NextOccurrenceFrom_DueBeforeCheckedDate_WithoutOccuranceOnCheckedDate_ReturnsNextOccurance()
        {
            var dueDate = new DateTime(2025, 1, 1);
            var checkedDate = new DateTime(2025, 1, 2);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Monthly
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.EqualTo(new DateTime(2025, 2, 1)));
        }

        [Test]
        public void Event_YearlyRepeat_OnFebruary29_OnlyEveryFourYears()
        {
            var dueDate = new DateTime(2024, 2, 29);
            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Yearly
            );
            Assert.That(@event.NextOccurrenceFrom(new DateTime(2026, 2, 1)), Is.EqualTo(new DateTime(2028, 2, 29)));
        }

        [Test]
        public void NextOccurrenceFrom_WhenEventIsComplete_ReturnsNull()
        {
            var dueDate = new DateTime(2025, 1, 11);
            var checkedDate = dueDate.AddDays(10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: true,
                RepeatInterval: RepeatInterval.Daily
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.Null);
        }

        [Test]
        public void NextOccurrenceFrom_WhenNoRepeat_AndCheckedDateAfterDueDate_ReturnsNull()
        {

            var dueDate = new DateTime(2025, 1, 11);
            var checkedDate = dueDate.AddDays(10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.Null);
        }
        [Test]
        public void NextOccurrenceFrom_WhenNoRepeat_AndCheckedDateBeforeDueDate_ReturnsDueDate()
        {

            var dueDate = new DateTime(2025, 1, 11);
            var checkedDate = dueDate.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.EqualTo(dueDate));
        }

        [Test]
        public void NextOccurrence_WhenDueToday_ReturnsToday()
        {
            var dueDate = DateTime.Today;

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.EqualTo(DateTime.Today));
        }

        [Test]
        public void NextOccurrence_WhenDueInFuture_ReturnsDueDate()
        {
            var dueDate = DateTime.Today.AddDays(10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.EqualTo(DateTime.Today.AddDays(10)));
        }

        [Test]
        public void NextOccurrence_WhenDueInPast_NoRepeat_ReturnsNull()
        {
            var dueDate = DateTime.Today.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.Null);
        }
        [Test]
        public void NextOccurrence_WhenDueInPast_DailyRepeat_ReturnsToday()
        {
            var dueDate = DateTime.Today.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );

            Assert.That(@event.NextOccurrence(), Is.EqualTo(DateTime.Today));
        }
        [Test]
        public void NextOccurrence_WhenDueInPast_YearlyRepeat_ReturnsNextYear()
        {
            var dueDate = DateTime.Today.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Yearly
            );

            Assert.That(@event.NextOccurrence(), Is.EqualTo(dueDate.AddYears(1)));
        }
        [Test]
        public void NextOccurrence_WhenDueInPast_AndWhenComplete_ReturnsNull()
        {
            var dueDate = DateTime.Today.AddDays(-10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: true,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.Null);
        }
        [Test]
        public void NextOccurrence_WhenDueToday_AndWhenComplete_ReturnsNull()
        {
            var dueDate = DateTime.Today;

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: true,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.Null);
        }
        [Test]
        public void NextOccurrence_WhenDueInFuture_AndWhenComplete_ReturnsNull()
        {
            var dueDate = DateTime.Today.AddDays(10);

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: true,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.NextOccurrence(), Is.Null);
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
        public void Occurances_CountOne_WhenNonRepeat()
        {
            var dueDate = DateTime.Today;

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.Occurances.Count(), Is.EqualTo(1));
        }

        [Test]
        public void FirstOccuranceIsDueDate_WhenNoRepeat()
        {
            var dueDate = DateTime.Today;

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            );

            Assert.That(@event.Occurances.First(), Is.EqualTo(dueDate));
        }
        [Test]
        public void FirstOccuranceIsDueDate_WhenRepeat()
        {
            var dueDate = DateTime.Today;

            var @event = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );

            Assert.That(@event.Occurances.First(), Is.EqualTo(dueDate));
        }
    }
}
