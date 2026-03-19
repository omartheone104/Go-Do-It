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
        public void DueToday_WhenDateIsToday_ReturnsTrue()
        {
            var TodayEvent = new Event(Id: 1, Title: "Today", Description: "Today Test Event", DueDate: DateTime.Today, CategoryId: 1, ParentId: null, IsComplete: false, RepeatInterval: null);

            Assert.That(TodayEvent.DueToday(), Is.True);
        }

        [Test]
        public void DueToday_WhenDateIsFuture_ReturnsFalse()
        {
            var FutureEvent = new Event(Id: 1, Title: "Future", Description: "Future Test Event", DueDate: DateTime.Today.AddDays(10), CategoryId: 1, ParentId: null, IsComplete: false, RepeatInterval: null);

            Assert.That(FutureEvent.DueToday(), Is.False);
        }

        [Test]
        public void DueToday_WhenDateIsPast_ReturnsFalse()
        {
            var PastEvent = new Event(Id: 1, Title: "Past", Description: "Past Test Event", DueDate: DateTime.Today.AddDays(-10), CategoryId: 1, ParentId: null, IsComplete: false, RepeatInterval: null);

            Assert.That(PastEvent.DueToday(), Is.False);
        }

        [AvaloniaTest]
        public void Check_Calendar()
        {
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            // Show the window, as it's required to get layout processed:
            window.Show();
            window.Find<Avalonia.Controls.Calendar>("calendar").SelectedDate = new DateTime(2026, 3, 18, 00, 00, 00, 000);
            Assert.That(window.Find<TextBlock>("calendar_text").Text, Is.EqualTo("3/18/2026 12:00:00 AM"));
        }

        [Test]
        public void CompletedEvent_IsNotDueToday()
        {
            var completedEvent = new Event(
                Id: 1, 
                Title: "Completed Task", 
                Description: "Test", 
                DueDate: DateTime.Today, 
                CategoryId: 1, 
                ParentId: null, 
                IsComplete: true, 
                RepeatInterval: null
            );

            Assert.That(completedEvent.DueToday(), Is.False);
        }
    }
}