using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace GoDoIt.Tests {
    [TestFixture]
    public class BasicTests {
        [Test]
        public void SaveAndLoad_String_Works() {
            using var stream = new MemoryStream();
            var data = Encoding.UTF8.GetBytes("Go Do It Test");
            stream.Write(data, 0, data.Length);

            stream.Position = 0;
            var loaded = new byte[stream.Length];
            stream.Read(loaded, 0, loaded.Length);

            string result = Encoding.UTF8.GetString(loaded);
            Assert.That(result, Is.EqualTo("Go Do It Test"));
        }
    }
}