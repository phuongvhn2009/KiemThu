using NUnit.Framework;
using OpenQA.Selenium;

namespace SauceDemo.Tests.Utilities;

public static class ScreenshotHelper
{
    public static void Capture(IWebDriver? driver, string directory)
    {
        if (driver is not ITakesScreenshot camera) return;
        var name = string.Concat(TestContext.CurrentContext.Test.Name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        var path = Path.Combine(directory, name + ".png");
        camera.GetScreenshot().SaveAsFile(path);
        TestContext.AddTestAttachment(path);
    }
}
