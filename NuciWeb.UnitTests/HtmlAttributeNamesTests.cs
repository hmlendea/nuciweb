using NUnit.Framework;

namespace NuciWeb.UnitTests;

[TestFixture]
public sealed class HtmlAttributeNamesTests
{
    [Test]
    public void GivenHtmlAttributeNames_WhenAccessingClass_ThenReturnsClassString()
        => Assert.That(HtmlAttributeNames.Class, Is.EqualTo("class"));

    [Test]
    public void GivenHtmlAttributeNames_WhenAccessingHyperlink_ThenReturnsHrefString()
        => Assert.That(HtmlAttributeNames.Hyperlink, Is.EqualTo("href"));

    [Test]
    public void GivenHtmlAttributeNames_WhenAccessingId_ThenReturnsIdString()
        => Assert.That(HtmlAttributeNames.Id, Is.EqualTo("id"));

    [Test]
    public void GivenHtmlAttributeNames_WhenAccessingName_ThenReturnsNameString()
        => Assert.That(HtmlAttributeNames.Name, Is.EqualTo("name"));

    [Test]
    public void GivenHtmlAttributeNames_WhenAccessingSource_ThenReturnsSrcString()
        => Assert.That(HtmlAttributeNames.Source, Is.EqualTo("src"));

    [Test]
    public void GivenHtmlAttributeNames_WhenAccessingStyle_ThenReturnsStyleString()
        => Assert.That(HtmlAttributeNames.Style, Is.EqualTo("style"));

    [Test]
    public void GivenHtmlAttributeNames_WhenAccessingValue_ThenReturnsValueString()
        => Assert.That(HtmlAttributeNames.Value, Is.EqualTo("value"));
}