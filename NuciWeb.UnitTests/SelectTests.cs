using NUnit.Framework;

namespace NuciWeb.UnitTests;

[TestFixture]
public sealed class SelectTests
{
    [Test]
    public void GivenSelect_WhenByClassWithValidName_ThenReturnsCorrectXPath()
        => Assert.That(
            Select.ByClass("test-class"),
            Is.EqualTo("//*[contains(concat(' ', normalize-space(@class), ' '), ' test-class ')]"));

    [Test]
    public void GivenSelect_WhenByIdWithValidId_ThenReturnsCorrectXPath()
        => Assert.That(
            Select.ById("test-id"),
            Is.EqualTo("//*[@id='test-id']"));

    [Test]
    public void GivenSelect_WhenByNameWithValidName_ThenReturnsCorrectXPath()
        => Assert.That(
            Select.ByName("test-name"),
            Is.EqualTo("//*[@name='test-name']"));

    [Test]
    public void GivenSelect_WhenByXPathWithValidXPath_ThenReturnsSameXPath()
        => Assert.That(
            Select.ByXPath("//div[@class='test']"),
            Is.EqualTo("//div[@class='test']"));

    [Test]
    public void GivenSelect_WhenByClassWithEmptyString_ThenReturnsXPathWithEmptyClass()
        => Assert.That(
            Select.ByClass(string.Empty),
            Is.EqualTo("//*[contains(concat(' ', normalize-space(@class), ' '), '  ')]"));

    [Test]
    public void GivenSelect_WhenByIdWithEmptyString_ThenReturnsXPathWithEmptyId()
        => Assert.That(
            Select.ById(string.Empty),
            Is.EqualTo("//*[@id='']"));

    [Test]
    public void GivenSelect_WhenByNameWithEmptyString_ThenReturnsXPathWithEmptyName()
        => Assert.That(
            Select.ByName(string.Empty),
            Is.EqualTo("//*[@name='']"));
}