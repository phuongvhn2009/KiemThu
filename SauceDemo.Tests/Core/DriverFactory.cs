using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using SauceDemo.Tests.Config;

namespace SauceDemo.Tests.Core;

public static class DriverFactory
{
    private enum BrowserKind { Chrome, Firefox, Edge, CocCoc }

    public static IWebDriver Create(TestConfig config)
    {
        if (!Enum.TryParse<BrowserKind>(config.Browser, true, out var browser) ||
            !Enum.IsDefined(browser) || int.TryParse(config.Browser, out _))
            throw new ArgumentException($"Unsupported browser '{config.Browser}'. Supported: {string.Join(", ", Enum.GetNames<BrowserKind>())}");

        var binary = Environment.GetEnvironmentVariable("BROWSER_BINARY") ?? Environment.GetEnvironmentVariable("CHROME_BINARY");
        var driverDirectory = Environment.GetEnvironmentVariable("CHROMEDRIVER_DIRECTORY");
        switch (browser)
        {
            case BrowserKind.Chrome:
            case BrowserKind.CocCoc:
                var chrome = new ChromeOptions();
                if (config.Headless) chrome.AddArgument("--headless=new");
                chrome.AddArguments($"--window-size={config.WindowWidth},{config.WindowHeight}", "--no-sandbox", "--disable-dev-shm-usage", "--incognito", "--disable-features=PasswordLeakDetection");
                chrome.AddUserProfilePreference("credentials_enable_service", false);
                chrome.AddUserProfilePreference("profile.password_manager_enabled", false);
                chrome.AddUserProfilePreference("profile.password_manager_leak_detection", false);
                if (browser == BrowserKind.CocCoc && string.IsNullOrWhiteSpace(binary))
                    binary = config.CocCocBinary;
                if (browser == BrowserKind.CocCoc && !File.Exists(binary))
                    throw new FileNotFoundException($"Browser binary not found: '{binary}'. Set CocCocBinary in configuration.", binary);
                if (!string.IsNullOrWhiteSpace(binary)) chrome.BinaryLocation = binary;
                return string.IsNullOrWhiteSpace(driverDirectory) ? new ChromeDriver(chrome)
                    : new ChromeDriver(ChromeDriverService.CreateDefaultService(driverDirectory), chrome);
            case BrowserKind.Edge:
                var edge = new EdgeOptions();
                if (config.Headless) edge.AddArgument("--headless=new");
                edge.AddArguments($"--window-size={config.WindowWidth},{config.WindowHeight}", "--no-sandbox", "--disable-dev-shm-usage", "--inprivate");
                if (!string.IsNullOrWhiteSpace(binary)) edge.BinaryLocation = binary;
                return new EdgeDriver(edge);
            default:
                var firefox = new FirefoxOptions();
                if (config.Headless) firefox.AddArgument("--headless");
                firefox.AddArguments($"--width={config.WindowWidth}", $"--height={config.WindowHeight}");
                if (!string.IsNullOrWhiteSpace(binary)) firefox.BinaryLocation = binary;
                return new FirefoxDriver(firefox);
        }
    }
}
