using NUnit.Framework;
using OpenQA.Selenium;

namespace SauceDemo.Tests.Utilities;

public static class ScreenshotHelper
{
    public static string? Capture(IWebDriver? driver, string directory, string? fileNameWithoutExtension = null)
    {
        if (driver is not ITakesScreenshot camera) return null;
        Directory.CreateDirectory(directory);
        var rawName = string.IsNullOrWhiteSpace(fileNameWithoutExtension)
            ? TestContext.CurrentContext.Test.Name
            : fileNameWithoutExtension;
        var invalidChars = Path.GetInvalidFileNameChars();
        var name = string.Concat(rawName.Select(c => invalidChars.Contains(c) ? '_' : c));
        var path = Path.Combine(directory, name + ".png");
        camera.GetScreenshot().SaveAsFile(path);
        TestContext.AddTestAttachment(path);
        return path;
    }
}
