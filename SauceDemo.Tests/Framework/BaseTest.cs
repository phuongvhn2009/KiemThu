using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SauceDemo.Tests.Pages;

namespace SauceDemo.Tests.Framework;

public abstract class BaseTest
{
    protected IWebDriver Driver = null!;
    protected Settings Config = null!;
    protected LoginPage Login = null!;
    private string directory = "";

    [SetUp]
    public void Start()
    {
        directory = Path.Combine(Environment.GetEnvironmentVariable("TEST_ARTIFACTS") ?? Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts"), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        Log("START " + TestContext.CurrentContext.Test.Name);
        Config = Settings.Load();
        var options = new ChromeOptions();
        if (Config.Headless) options.AddArgument("--headless=new");
        options.AddArguments("--window-size=1440,1000", "--no-sandbox", "--disable-dev-shm-usage");
        var binary = Environment.GetEnvironmentVariable("CHROME_BINARY");
        if (Config.Browser == "coccoc" && string.IsNullOrWhiteSpace(binary))
            binary = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "CocCoc", "Browser", "Application", "browser.exe");
        if (!string.IsNullOrWhiteSpace(binary)) options.BinaryLocation = binary;
        var driverDirectory = Environment.GetEnvironmentVariable("CHROMEDRIVER_DIRECTORY");
        Driver = string.IsNullOrWhiteSpace(driverDirectory)
            ? new ChromeDriver(options)
            : new ChromeDriver(ChromeDriverService.CreateDefaultService(driverDirectory), options);
        Login = new LoginPage(Driver, Config.TimeoutSeconds);
        Login.Open(Config.BaseUrl);
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
            if (result.Outcome.Status == TestStatus.Failed && Driver is ITakesScreenshot camera)
            {
                var path = Path.Combine(directory, string.Concat(TestContext.CurrentContext.Test.Name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)) + ".png");
                camera.GetScreenshot().SaveAsFile(path);
                TestContext.AddTestAttachment(path);
            }
        }
        catch (Exception e) { TestContext.Error.WriteLine("Artifact capture failed: " + e.Message); }
        finally { Driver?.Quit(); Driver?.Dispose(); }
    }
}
