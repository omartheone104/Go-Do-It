using NUnit.Framework;
using System.IO;
using System.Text;
using System.Text.Json;
using GoDoIt;

namespace GoDoIt.Tests;

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
