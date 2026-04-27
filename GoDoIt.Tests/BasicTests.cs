using NUnit.Framework;
using Avalonia.Media;
using GoDoIt;

namespace GoDoIt.Tests;

[TestFixture]
public class BasicTests
{
    [Test]
    public void SaveAndLoad_String_Works()
    {
        using var stream = new System.IO.MemoryStream();
        var data = System.Text.Encoding.UTF8.GetBytes("Go Do It Test");
        stream.Write(data, 0, data.Length);

        stream.Position = 0;
        var loaded = new byte[stream.Length];
        stream.Read(loaded, 0, loaded.Length);

        string result = System.Text.Encoding.UTF8.GetString(loaded);
        Assert.That(result, Is.EqualTo("Go Do It Test"));
    }

    [Test]
    public void DifferentCategory_DifferentIds()
    {
        var category1 = new Category("Test Cat", Color.FromRgb(122, 122, 122));
        var category2 = new Category("Test Cat", Color.FromRgb(122, 122, 122));
        Assert.That(category1.Id, Is.Not.EqualTo(category2.Id));
    }
}
