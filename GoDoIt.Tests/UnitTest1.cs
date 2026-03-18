using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.IO;
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
    }
}