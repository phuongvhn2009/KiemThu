using System.Diagnostics;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using SauceDemo.Tests.Config;
using SauceDemo.Tests.Pages;
using SauceDemo.Tests.Tests;
using SauceDemo.Tests.Utilities;

namespace SauceDemo.Tests.Core;

public abstract class BaseTest
{
    protected IWebDriver Driver = null!;
    protected TestConfig Config = null!;
    protected LoginPage Login = null!;
    private string directory = "";
    private string logFilePath = "";
    private Stopwatch stopwatch = null!;

    [SetUp]
    public void Start()
    {
        Driver = null!;
        stopwatch = Stopwatch.StartNew();
        var artifactsRoot = Environment.GetEnvironmentVariable("TEST_ARTIFACTS")
            ?? Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts");
        directory = Path.Combine(artifactsRoot, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        logFilePath = Path.Combine(directory, "test.log");

        var test = TestContext.CurrentContext.Test;
        Config = ConfigReader.Load();
        Utilities.Log.Info(
            $"START {test.Name} | Fixture={GetType().Name} | Browser={Config.Browser} | BaseUrl={Config.BaseUrl}",
            logFilePath);

        Driver = DriverFactory.Create(Config);
        Login = new LoginPage(Driver, Config.TimeoutSeconds);
        try
        {
            Login.Open(Config.BaseUrl);
        }
        catch
        {
            try { Driver.Quit(); } finally { Driver.Dispose(); Driver = null!; }
            throw;
        }
    }

    [TearDown]
    public void Stop()
    {
        stopwatch?.Stop();
        var durationMs = stopwatch?.ElapsedMilliseconds ?? 0;
        var test = TestContext.CurrentContext.Test;
        var currentUrl = Config?.BaseUrl ?? "";
        try
        {
            if (Driver is not null && !string.IsNullOrWhiteSpace(Driver.Url))
                currentUrl = Driver.Url;
        }
        catch { }

        try
        {
            var result = TestContext.CurrentContext.Result;
            var isFailed = result.Outcome.Status == TestStatus.Failed;
            var status = isFailed ? "FAIL" : "PASS";
            var compactError = CompactSingleLine(result.Message);
            var screenshotPath = "";

            if (isFailed)
            {
                screenshotPath = ScreenshotHelper.Capture(Driver, directory) ?? "";
                Utilities.Log.Error(
                    $"ROT {test.Name} ({durationMs}ms) | URL={currentUrl} | Error={compactError} | Screenshot={screenshotPath}",
                    logFilePath);
            }
            else
            {
                Utilities.Log.Info($"OK {test.Name} ({durationMs}ms) | URL={currentUrl}", logFilePath);
            }

            var categories = string.Join(", ",
                GetType().GetCustomAttributes(typeof(CategoryAttribute), inherit: true)
                    .OfType<CategoryAttribute>()
                    .Select(c => c.Name)
                    .Concat(test.Properties["Category"].OfType<string>())
                    .Distinct(StringComparer.OrdinalIgnoreCase));

            var scenario = test.Properties["Description"].OfType<string>().FirstOrDefault() ?? "";
            var account = test.Arguments.FirstOrDefault() is LoginCase loginCase
                ? (string.IsNullOrEmpty(loginCase.Username) ? "(empty)" : loginCase.Username)
                : "";

            TestReportCollector.Record(new TestRunRecord(
                TestName: test.Name,
                FixtureName: GetType().Name,
                Categories: categories,
                Scenario: scenario,
                Account: account,
                Url: currentUrl,
                Status: status,
                DurationMs: durationMs,
                ErrorMessage: compactError,
                ScreenshotPath: screenshotPath,
                FinishedAt: DateTime.UtcNow));
        }
        catch (Exception e)
        {
            Utilities.Log.Error($"Artifact capture failed for {test.Name}: {e.Message}", logFilePath);
        }
        finally
        {
            try { Driver?.Quit(); }
            finally { Driver?.Dispose(); }
        }
    }

    private static string CompactSingleLine(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? ""
            : string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
