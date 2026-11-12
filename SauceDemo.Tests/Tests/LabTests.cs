using NUnit.Framework;
using SauceDemo.Tests.Framework;
using SauceDemo.Tests.Pages;

namespace SauceDemo.Tests.Tests;

public class LabTests : BaseTest
{
    [Test]
    public void Products()
    {
        Login.Login("standard_user", "secret_sauce");
        var products = new InventoryPage(Driver, Config.TimeoutSeconds).Products();
        Assert.That(products, Has.Count.EqualTo(6));
        foreach (var item in products)
        {
            TestContext.Out.WriteLine($"{item.Name}: {item.Price}");
            Assert.That(item.Name, Is.Not.Empty);
            Assert.That(item.Price, Does.StartWith("$"));
        }
        Assert.That(products.Select(p => p.Name), Does.Contain("Sauce Labs Backpack"));
    }
    [Test]
    public void PricesAscending()
    {
        Login.Login("standard_user", "secret_sauce");
        var page = new InventoryPage(Driver, Config.TimeoutSeconds);
        page.SortPrice();
        var prices = page.Products().Select(p => decimal.Parse(p.Price.TrimStart('$'), System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        Assert.That(prices, Has.Length.EqualTo(6));
        Assert.That(prices, Is.Ordered.Ascending);
    }
    [Test]
    public void DynamicLoading()
    {
        var page = new LabPage(Driver, Config.TimeoutSeconds);
        page.Open("dynamic_loading/2"); page.StartLoading();
        Assert.That(page.Finish(true), Is.EqualTo("Hello World!"));
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "dynamic-loading-pass.png");
        ((OpenQA.Selenium.ITakesScreenshot)Driver).GetScreenshot().SaveAsFile(path);
        TestContext.AddTestAttachment(path);
    }
    [Test, Explicit("Intentional failure: capture Lab 3 evidence separately")]
    public void DynamicLoadingWithoutWait()
    {
        var page = new LabPage(Driver, Config.TimeoutSeconds);
        page.Open("dynamic_loading/2"); page.StartLoading();
        Assert.That(page.Finish(false), Is.EqualTo("Hello World!"));
    }
    [TestCase("jsAlert", "You successfully clicked an alert")]
    [TestCase("jsConfirm", "You clicked: Cancel")]
    [TestCase("jsPrompt", "You entered: Hello Lab3")]
    public void JavaScriptAlert(string kind, string expected)
    {
        var page = new LabPage(Driver, Config.TimeoutSeconds);
        page.Open("javascript_alerts");
        Assert.That(page.Alert(kind, "Hello Lab3"), Is.EqualTo(expected));
    }
    [Test]
    public void Iframe()
    {
        var page = new LabPage(Driver, Config.TimeoutSeconds);
        page.Open("iframe");
        Assert.That(page.EditFrame("Hello Lab3"), Is.EqualTo("Hello Lab3"));
        Assert.That(Driver.FindElement(OpenQA.Selenium.By.TagName("h3")).Text, Does.Contain("iFrame"));
    }
}
