using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using SauceDemo.Tests.Config;
using SauceDemo.Tests.Utilities;
using SauceDemo.Tests.Pages;

namespace SauceDemo.Tests.Core;

public abstract class BaseTest
{
    protected IWebDriver Driver = null!;
    protected TestConfig Config = null!;
    protected LoginPage Login = null!;
    private string directory = "";

    [SetUp]
    public void Start()
    {
        Driver = null!;
        directory = Path.Combine(Environment.GetEnvironmentVariable("TEST_ARTIFACTS") ?? Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts"), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        Log("START " + TestContext.CurrentContext.Test.Name);
        Config = ConfigReader.Load();
        Driver = DriverFactory.Create(Config);
        Login = new LoginPage(Driver, Config.TimeoutSeconds);
        try { Login.Open(Config.BaseUrl); }
        catch
        {
            try { Driver.Quit(); } finally { Driver.Dispose(); Driver = null!; }
            throw;
        }
    }

    protected void Log(string message)
    {
        var line = $"{DateTime.UtcNow:O} {message}";
        TestContext.Progress.WriteLine(line);
        File.AppendAllText(Path.Combine(directory, "test.log"), line + Environment.NewLine);
    }

    [TearDown]
    public void Stop()
    {
        try
        {
            var result = TestContext.CurrentContext.Result;
            Log($"END {result.Outcome} {result.Message}");
            if (result.Outcome.Status == TestStatus.Failed)
                ScreenshotHelper.Capture(Driver, directory);
        }
        catch (Exception e) { TestContext.Error.WriteLine("Artifact capture failed: " + e.Message); }
        finally { try { Driver?.Quit(); } finally { Driver?.Dispose(); } }
    }
}
