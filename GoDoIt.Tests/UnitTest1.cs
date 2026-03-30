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
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: Guid.NewGuid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            ),
            new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: Guid.NewGuid(),
                ParentId: null,
                IsComplete: true,
                RepeatInterval: RepeatInterval.None
            ),
            new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: Guid.NewGuid(),
                ParentId: Guid.NewGuid(),
                IsComplete: false,
                RepeatInterval: RepeatInterval.None
            ),
            new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: Guid.NewGuid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Daily
            ),
            new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: Guid.NewGuid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Weekly
            ),
            new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: Guid.NewGuid(),
                ParentId: null,
                IsComplete: false,
                RepeatInterval: RepeatInterval.Monthly
            ),
            new Event(
                Title: "Test Task",
                Description: "Test",
                DueDate: DateTime.Today,
                CategoryId: Guid.NewGuid(),
                ParentId: null,
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
}
