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
using Avalonia.Media;
using System.Text.Json;
using GoDoIt;
using Ical.Net.Serialization;
using Ical.Net.CalendarComponents;


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

        [Test]
        public void DifferentCategory_DifferentIds()
        {
            var category1 = new Category("Test Cat", Color.FromRgb(122, 122, 122));
            var category2 = new Category("Test Cat", Color.FromRgb(122, 122, 122));
            // Console.WriteLine(category2);
            Assert.That(category1.Id, Is.Not.EqualTo(category2.Id));
        }
    }

    [TestFixture]
    public class EventTests
    {
        private static readonly Event[] testEvents = [
            new Event(
                    Title: "Test Task 1",
                    Description: "Test Description 1",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.None
            ),
            new Event(
                    Title: "Test Task 2",
                    Description: "Test Description 2",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Daily
            ),
            new Event(
                    Title: "Test Task 3",
                    Description: "Test Description 3",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Weekly
            ),
            new Event(
                    Title: "Test Task 4",
                    Description: "Test Description 4",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Monthly
            ),
            new Event(
                    Title: "Test Task 5",
                    Description: "Test Description 5",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Yearly
            ),
            new Event(
                    Title: "Test Task 6",
                    Description: "Test Description 6",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.None
            ),
            new Event(
                    Title: "Test Task 7",
                    Description: "Test Description 7",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Daily
            ),
            new Event(
                    Title: "Test Task 8",
                    Description: "Test Description 8",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Weekly
            ),
            new Event(
                    Title: "Test Task 9",
                    Description: "Test Description 9",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Monthly
            ),
            new Event(
                    Title: "Test Task 10",
                    Description: "Test Description 10",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Yearly
            ),
            new Event(
                    Title: "Test Task 11",
                    Description: "Test Description 11",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.None
            ),
            new Event(
                    Title: "Test Task 12",
                    Description: "Test Description 12",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Daily
            ),
            new Event(
                    Title: "Test Task 13",
                    Description: "Test Description 13",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Weekly
            ),
            new Event(
                    Title: "Test Task 14",
                    Description: "Test Description 14",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Monthly
            ),
            new Event(
                    Title: "Test Task 15",
                    Description: "Test Description 15",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Yearly
            ),
            new Event(
                    Title: "Test Task 16",
                    Description: "Test Description 16",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.None
            ),
            new Event(
                    Title: "Test Task 17",
                    Description: "Test Description 17",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Daily
            ),
            new Event(
                    Title: "Test Task 18",
                    Description: "Test Description 18",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Weekly
            ),
            new Event(
                    Title: "Test Task 19",
                    Description: "Test Description 19",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Monthly
            ),
            new Event(
                    Title: "Test Task 20",
                    Description: "Test Description 20",
                    DueDate: DateTime.Today,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Yearly
            ),
            new Event(
                    Title: "Test Task 21",
                    Description: "Test Description 21",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(253222763).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.None
            ),
            new Event(
                    Title: "Test Task 22",
                    Description: "Test Description 22",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(1881907738).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Daily
            ),
            new Event(
                    Title: "Test Task 23",
                    Description: "Test Description 23",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(3503226702).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Weekly
            ),
            new Event(
                    Title: "Test Task 24",
                    Description: "Test Description 24",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(3331792913).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Monthly
            ),
            new Event(
                    Title: "Test Task 25",
                    Description: "Test Description 25",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(366397947).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Yearly
            ),
            new Event(
                    Title: "Test Task 26",
                    Description: "Test Description 26",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(838917024).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.None
            ),
            new Event(
                    Title: "Test Task 27",
                    Description: "Test Description 27",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(2944912136).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Daily
            ),
            new Event(
                    Title: "Test Task 28",
                    Description: "Test Description 28",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(782037370).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Weekly
            ),
            new Event(
                    Title: "Test Task 29",
                    Description: "Test Description 29",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(931164482).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Monthly
            ),
            new Event(
                    Title: "Test Task 30",
                    Description: "Test Description 30",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(3176260235).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: null,
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Yearly
            ),
            new Event(
                    Title: "Test Task 31",
                    Description: "Test Description 31",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(1863100829).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.None
            ),
            new Event(
                    Title: "Test Task 32",
                    Description: "Test Description 32",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(815242392).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Daily
            ),
            new Event(
                    Title: "Test Task 33",
                    Description: "Test Description 33",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(3935534374).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Weekly
            ),
            new Event(
                    Title: "Test Task 34",
                    Description: "Test Description 34",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(1648655535).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Monthly
            ),
            new Event(
                    Title: "Test Task 35",
                    Description: "Test Description 35",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(386828443).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: true,
                    RepeatInterval: RepeatInterval.Yearly
            ),
            new Event(
                    Title: "Test Task 36",
                    Description: "Test Description 36",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(381891601).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.None
            ),
            new Event(
                    Title: "Test Task 37",
                    Description: "Test Description 37",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(4264577284).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Daily
            ),
            new Event(
                    Title: "Test Task 38",
                    Description: "Test Description 38",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(315763967).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Weekly
            ),
            new Event(
                    Title: "Test Task 39",
                    Description: "Test Description 39",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(454790657).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Monthly
            ),
            new Event(
                    Title: "Test Task 40",
                    Description: "Test Description 40",
                    DueDate: DateTimeOffset.FromUnixTimeSeconds(1813843450).UtcDateTime,
                    CategoryId: Guid.NewGuid(),
                    ParentId: Guid.NewGuid(),
                    IsComplete: false,
                    RepeatInterval: RepeatInterval.Yearly
            ),
        ];

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

        [Test]
        public void IdenticalEvents_DifferentIds()
        {
            var event1 = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );
            var event2 = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );
            Assert.That(event1.Id, Is.Not.EqualTo(event2.Id));
        }

        [Test]
        public void EventsWithSameDetails_NotEqual()
        {
            var event1 = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );
            var event2 = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );
            Assert.That(event1, Is.Not.EqualTo(event2));
            Assert.That(event1.GetHashCode(), Is.Not.EqualTo(event2.GetHashCode()));
        }

        [Test]
        public void IdenticalEvents_EqualProperties()
        {
            var event1 = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );
            var event2 = new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: new Guid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            );

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
        public void ToCalendarEvent_RoundTrip_ReturnsIdenticalObject([ValueSource(nameof(testEvents))] Event event1)
        {
            var calEvent = event1.AsCalendarEvent();

            var event2 = Event.FromCalendarEvent(calEvent);

            Assert.That(event1, Is.EqualTo(event2));
        }

        [Test]
        public void ToCalendarEvent_RoundTripToString_ReturnsIdenticalObject([ValueSource(nameof(testEvents))] Event event1)
        {
            var calEvent1 = event1.AsCalendarEvent();

            var calString = new CalendarSerializer().SerializeToString(calEvent1);
            Assert.That(calString, Is.Not.Null);

            var calEvent2 = Ical.Net.Calendar.Load<CalendarEvent>(calString).FirstOrDefault(); // There should only ever be one object
            Assert.That(calEvent2, Is.Not.Null);

            var event2 = Event.FromCalendarEvent(calEvent2);
            Assert.That(event1, Is.EqualTo(event2));
        }
    }

    [TestFixture]
    public class JsonTests
    {
        [TestCase("""{"Id":"894cba1d-e8f7-46bf-b97d-9cf3abddb553","Title":"Base Task","Description":"Test","DueDate":"2025-01-01T00:00:00-05:00","CategoryId":"7bd38a43-2e24-4125-872b-cd9ffbbc2022","ParentId":null,"IsComplete":false,"RepeatInterval":"None"}""")]
        [TestCase("""{"Id":"9697f49f-e128-4384-b456-83a53bc6cc8d","Title":"Daily Task","Description":"Test","DueDate":"2025-01-01T00:00:00-05:00","CategoryId":"5e4daec0-9038-4cf2-ad66-4f9727fd56ad","ParentId":null,"IsComplete":false,"RepeatInterval":"Daily"}""")]
        [TestCase("""{"Id":"2ac53662-e26e-420c-9bc6-1dcd2f12369d","Title":"Weekly Task","Description":"Test","DueDate":"2025-01-01T00:00:00-05:00","CategoryId":"40ae8011-9bd9-44db-9939-5539f0320343","ParentId":null,"IsComplete":false,"RepeatInterval":"Weekly"}""")]
        [TestCase("""{"Id":"e8fa2341-c7d9-465a-bae6-a51ac44f34dc","Title":"Monthly Task","Description":"Test","DueDate":"2025-01-01T00:00:00-05:00","CategoryId":"ef9b549e-981b-46f1-8576-1ec5a8700753","ParentId":null,"IsComplete":false,"RepeatInterval":"Monthly"}""")]
        [TestCase("""{"Id":"bb63ca16-0cc5-45ce-8850-4ba696f31a2c","Title":"Yearly Task","Description":"Test","DueDate":"2025-01-01T00:00:00-05:00","CategoryId":"192e2670-2853-4925-8f0e-fff511840a6e","ParentId":null,"IsComplete":false,"RepeatInterval":"Yearly"}""")]
        [TestCase("""{"Id":"9ad54246-b24b-4bcf-bafa-7ed848e20e6c","Title":"Completed Task","Description":"Test","DueDate":"2025-01-01T00:00:00-05:00","CategoryId":"a45b1ae0-7666-4c64-9b60-dc7aabcaca96","ParentId":null,"IsComplete":true,"RepeatInterval":"None"}""")]
        [TestCase("""{"Id":"6480f4ba-40d5-41cd-8a9f-d13a24de24c6","Title":"Child Task","Description":"Test","DueDate":"2025-01-01T00:00:00-05:00","CategoryId":"749faa0a-e182-44c6-8c2b-11fca9150543","ParentId":"749faa0a-e182-44c6-8c2b-11fca9150543","IsComplete":false,"RepeatInterval":"None"}""")]
        public void JsonRoundTrip_Event_ValidJson_ReturnsIdenticalObject(string json)
        {
            Event? event1;
            using (var readStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                event1 = JsonSerializer.Deserialize<Event>(readStream);
            }

            Assert.That(event1, Is.Not.Null);

            Event? event2;
            using (var writeStream = new MemoryStream())
            {
                JsonSerializer.Serialize(writeStream, event1);

                writeStream.Position = 0;
                event2 = JsonSerializer.Deserialize<Event>(writeStream);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(event2, Is.Not.Null);
                Assert.That(event2, Is.EqualTo(event1));
            }
        }

        [TestCase("""{"Id":"1df3dbd7-2c8a-42f8-8fc4-c8ef220b15e5","Name":"Test Category","Color":"#ff7a7a7a"}""")]
        public void JsonRoundTrip_Category_ValidJson_ReturnsIdenticalObject(string json)
        {
            Category? category1;
            using (var readStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                category1 = JsonSerializer.Deserialize<Category>(readStream);
            }

            Assert.That(category1, Is.Not.Null);

            Category? category2;
            using (var writeStream = new MemoryStream())
            {
                JsonSerializer.Serialize(writeStream, category1);

                writeStream.Position = 0;
                category2 = JsonSerializer.Deserialize<Category>(writeStream);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(category2, Is.Not.Null);
                Assert.That(category2, Is.EqualTo(category1));
            }
        }
    }

    [TestFixture]
    public class CategoryTests
    {
        [Test]
        public void Category_NameIsPreserved()
        {
            var category = new Category("Homework", Avalonia.Media.Colors.LightBlue);
            Assert.That(category.Name, Is.EqualTo("Homework"));
        }

        [Test]
        public void Category_ColorIsPreserved()
        {
            var category = new Category("Homework", Avalonia.Media.Colors.LightBlue);
            Assert.That(category.Color, Is.EqualTo(Avalonia.Media.Colors.LightBlue));
        }

        [Test]
        public void Category_TwoWithSameName_HaveDifferentIds()
        {
            var cat1 = new Category("Homework", Avalonia.Media.Colors.LightBlue);
            var cat2 = new Category("Homework", Avalonia.Media.Colors.LightBlue);
            Assert.That(cat1.Id, Is.Not.EqualTo(cat2.Id));
        }
    }

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
        public void Constructor_TwoEvents_HaveDifferentIds()
        {
            var ev1 = new Event("A", "Test", DateTime.Today, new Guid());
            var ev2 = new Event("B", "Test", DateTime.Today, new Guid());
            Assert.That(ev1.Id, Is.Not.EqualTo(ev2.Id));
        }
    }

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

        [Test]
        public void NextOccurrenceFrom_WeeklyRepeat_ReturnsCorrectDate()
        {
            var dueDate = new DateTime(2025, 1, 1); // Wednesday
            var checkedDate = new DateTime(2025, 1, 3); // Friday

            var @event = new Event(
                Title: "Test",
                Description: "Test",
                DueDate: dueDate,
                CategoryId: new Guid(),
                RepeatInterval: RepeatInterval.Weekly
            );

            Assert.That(@event.NextOccurrenceFrom(checkedDate), Is.EqualTo(new DateTime(2025, 1, 8)));
        }
    }

    [TestFixture]
    public class EventViewModelTests
    {
        [Test]
        public void EventViewModel_ColorFromMatchingCategory()
        {
            var cat = new Category("Work", Avalonia.Media.Colors.LightPink);
            var ev = new Event("Test", "Test", DateTime.Today, cat.Id);
            var evm = new EventViewModel(ev, new[] { cat });
            Assert.That(evm.Color, Is.EqualTo(Avalonia.Media.Colors.LightPink));
        }

        [Test]
        public void EventViewModel_ColorFallsBackToLightGray_WhenCategoryNotFound()
        {
            var ev = new Event("Test", "Test", DateTime.Today, Guid.NewGuid());
            var evm = new EventViewModel(ev, Array.Empty<Category>());
            Assert.That(evm.Color, Is.EqualTo(Avalonia.Media.Colors.LightGray));
        }

        [Test]
        public void EventViewModel_TitleMatchesEvent()
        {
            var cat = new Category("Work", Avalonia.Media.Colors.LightBlue);
            var ev = new Event("My Title", "Test", DateTime.Today, cat.Id);
            var evm = new EventViewModel(ev, new[] { cat });
            Assert.That(evm.Title, Is.EqualTo("My Title"));
        }

        [Test]
        public void EventViewModel_DescriptionMatchesEvent()
        {
            var cat = new Category("Work", Avalonia.Media.Colors.LightBlue);
            var ev = new Event("Test", "My Description", DateTime.Today, cat.Id);
            var evm = new EventViewModel(ev, new[] { cat });
            Assert.That(evm.Description, Is.EqualTo("My Description"));
        }

        [Test]
        public void EventViewModel_DueDateMatchesEvent()
        {
            var date = new DateTime(2026, 5, 10);
            var cat = new Category("Work", Avalonia.Media.Colors.LightBlue);
            var ev = new Event("Test", "Test", date, cat.Id);
            var evm = new EventViewModel(ev, new[] { cat });
            Assert.That(evm.DueDate, Is.EqualTo(date));
        }
    }

    [TestFixture]
    public class MainWindowViewModelTests
    {
        private string tempData;
        private string tempBackup;

        [OneTimeSetUp]
        public void StashUserData()
        {
            tempData = Path.GetTempFileName();
            tempBackup = Path.GetTempFileName();

            if (File.Exists(StorageService.DataFile))
            {
                File.Move(StorageService.DataFile, tempData, overwrite: true);
            }
            if (File.Exists(StorageService.BackupFile))
            {
                File.Move(StorageService.BackupFile, tempBackup, overwrite: true);
            }
        }

        [OneTimeTearDown]
        public void RestoreUserData()
        {
            File.Move(tempData, StorageService.DataFile, overwrite: true);
            File.Move(tempBackup, StorageService.BackupFile, overwrite: true);

            File.Delete(tempData);
            File.Delete(tempBackup);
        }

        [AvaloniaTest]
        public void Check_Calendar()
        {
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(loadFromDisk: false)
            };

            // Show the window, as it's required to get layout processed:
            var date = new DateTime(2026, 3, 18, 00, 00, 00, 000);
            window.Show();
            Assert.That(window.Find<Avalonia.Controls.Calendar>("calendar"), Is.Not.Null);
            window.Find<Avalonia.Controls.Calendar>("calendar").SelectedDate = date;
            Assert.That(window.Find<TextBlock>("calendar_text"), Is.Not.Null);
            Assert.That(window.Find<TextBlock>("calendar_text").Text, Is.EqualTo(date.ToString()));
        }
        [Test]
        public void Constructor_CategoriesInitialized()
        {
            var vm = new MainWindowViewModel(loadFromDisk: false);
            Assert.That(vm.Categories.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Constructor_TasksEmpty()
        {
            var vm = new MainWindowViewModel(loadFromDisk: false);
            Assert.That(vm.Tasks.Count, Is.EqualTo(0));
        }

        [Test]
        public void SaveTask_AddsToTasksAndTaskViews()
        {
            var vm = new MainWindowViewModel(loadFromDisk: false);
            vm.NewTask.Title = "New Task";
            vm.NewTask.Description = "Test";
            vm.NewTask.Category = vm.Categories[0];
            vm.SaveTaskCommand.Execute(null);
            Assert.That(vm.Tasks.Count, Is.EqualTo(1));
            Assert.That(vm.TaskViews.Count, Is.EqualTo(1));
        }

        [Test]
        public void SaveTask_DoesNothing_WhenTitleEmpty()
        {
            var vm = new MainWindowViewModel(loadFromDisk: false);
            vm.NewTask.Title = "";
            vm.NewTask.Category = vm.Categories[0];
            vm.SaveTaskCommand.Execute(null);
            Assert.That(vm.Tasks.Count, Is.EqualTo(0));
        }

        [Test]
        public void SaveTask_DoesNothing_WhenCategoryNull()
        {
            var vm = new MainWindowViewModel(loadFromDisk: false);
            vm.NewTask.Title = "Test";
            vm.NewTask.Category = null;
            vm.SaveTaskCommand.Execute(null);
            Assert.That(vm.Tasks.Count, Is.EqualTo(0));
        }

        [Test]
        public void SaveTask_ResetsNewTask_AfterSaving()
        {
            var vm = new MainWindowViewModel(loadFromDisk: false);
            vm.NewTask.Title = "New Task";
            vm.NewTask.Category = vm.Categories[0];
            vm.SaveTaskCommand.Execute(null);
            Assert.That(vm.NewTask.Title, Is.EqualTo(string.Empty));
        }

        [Test]
        public void SaveTask_TaskViewColorMatchesCategory()
        {
            var vm = new MainWindowViewModel(loadFromDisk: false);
            vm.NewTask.Title = "Test";
            vm.NewTask.Category = vm.Categories[0];
            vm.SaveTaskCommand.Execute(null);
            Assert.That(vm.TaskViews[0].Color, Is.EqualTo(vm.Categories[0].Color));
        }
    }
}

[TestFixture]
class CategoryTests
{
    public void CategoryRoundTripPerservesData()
    {
        var category1 = new Category("Test Category", Colors.LightBlue);
        var property = category1.AsCalendarProperty();

        var category2 = Category.FromCalendarProperty(property);

        Assert.That(category1, Is.EqualTo(category2));
    }
}
