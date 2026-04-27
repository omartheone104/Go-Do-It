using NUnit.Framework;
using Avalonia.Media;
using GoDoIt;

namespace GoDoIt.Tests;

[TestFixture]
public class CategoryTests
{
    [Test]
    public void Category_NameIsPreserved()
    {
        var category = new Category("Homework", Colors.LightBlue);
        Assert.That(category.Name, Is.EqualTo("Homework"));
    }

    [Test]
    public void Category_ColorIsPreserved()
    {
        var category = new Category("Homework", Colors.LightBlue);
        Assert.That(category.Color, Is.EqualTo(Colors.LightBlue));
    }

    [Test]
    public void Category_TwoWithSameName_HaveDifferentIds()
    {
        var cat1 = new Category("Homework", Colors.LightBlue);
        var cat2 = new Category("Homework", Colors.LightBlue);
        Assert.That(cat1.Id, Is.Not.EqualTo(cat2.Id));
    }

    [Test]
    public void CategoryRoundTrip_PreservesData()
    {
        var category1 = new Category("Test Category", Colors.LightBlue);
        var property = category1.AsCalendarProperty();

        var category2 = Category.FromCalendarProperty(property);

        Assert.That(category1, Is.EqualTo(category2));
    }
}
