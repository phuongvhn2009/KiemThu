using NUnit.Framework;
using SauceDemo.Tests.Core;
using SauceDemo.Tests.Pages;
using SauceDemo.Tests.Utilities;

namespace SauceDemo.Tests.Tests;

[TestFixture, Category("Lab")]
public class LabTests : BaseTest
{
    [Test, Category("Slow"), Description("Kich ban Explicit Wait cho phan tu dong #finish xuat hien")]
    public void DynamicLoading()
    {
        var page = new LabPage(Driver, Config.TimeoutSeconds, Config.LabBaseUrl);
        page.Open("dynamic_loading/2");
        page.StartLoading();
        Assert.That(page.Finish(true), Is.EqualTo("Hello World!"));
        Assert.That(page.Url, Does.EndWith("/dynamic_loading/2"));
        ScreenshotHelper.Capture(Driver, TestContext.CurrentContext.WorkDirectory, "dynamic-loading-pass");
    }

    [Test, Explicit("Intentional failure: capture Lab 3 and Lab 6 ERROR log evidence separately"), Category("Slow")]
    [Description("Kich ban co y khong cho #finish de chung minh loi NoSuchElementException va log ERROR")]
    public void DynamicLoadingWithoutWait()
    {
        var page = new LabPage(Driver, Config.TimeoutSeconds, Config.LabBaseUrl);
        page.Open("dynamic_loading/2");
        page.StartLoading();
        Assert.That(page.Finish(false), Is.EqualTo("Hello World!"));
    }

    [TestCase("jsAlert", "You successfully clicked an alert")]
    [TestCase("jsConfirm", "You clicked: Cancel")]
    [TestCase("jsPrompt", "You entered: Hello Lab3")]
    [Description("Kich ban xu ly 3 loai JavaScript Alert/Confirm/Prompt va Assert ket qua")]
    public void JavaScriptAlert(string kind, string expected)
    {
        var page = new LabPage(Driver, Config.TimeoutSeconds, Config.LabBaseUrl);
        page.Open("javascript_alerts");
        var expectedText = kind == "jsAlert" ? "I am a JS Alert"
            : kind == "jsConfirm" ? "I am a JS Confirm" : "I am a JS prompt";
        Assert.That(page.OpenAlert(kind), Is.EqualTo(expectedText));
        if (kind == "jsConfirm") page.DismissAlert();
        else if (kind == "jsPrompt") page.AnswerPrompt("Hello Lab3");
        else page.AcceptAlert();
        Assert.That(page.Result, Is.EqualTo(expected));
        Assert.That(page.Url, Does.EndWith("/javascript_alerts"));
    }

    [Test, Description("Kich ban chuyen vao Iframe nhap noi dung va quay ve DefaultContent")]
    public void Iframe()
    {
        var page = new LabPage(Driver, Config.TimeoutSeconds, Config.LabBaseUrl);
        page.Open("iframe");
        Assert.That(page.EditFrame("Hello Lab3"), Is.EqualTo("Hello Lab3"));
        Assert.That(page.Heading, Does.Contain("iFrame"));
        Assert.That(page.Url, Does.EndWith("/iframe"));
    }
}
