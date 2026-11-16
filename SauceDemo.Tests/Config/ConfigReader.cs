using Microsoft.Extensions.Configuration;

namespace SauceDemo.Tests.Config;

public static class ConfigReader
{
    public static TestConfig Load()
    {
        var source = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Config/appsettings.json", optional: false).Build();
        using var lifetime = source as IDisposable;
        var config = source.Get<TestConfig>() ?? throw new InvalidDataException("Missing configuration");
        config.Browser = Environment.GetEnvironmentVariable("TEST_BROWSER") ?? config.Browser;
        foreach (var url in new[] { config.BaseUrl, config.LabBaseUrl })
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                throw new InvalidDataException($"Expected an absolute HTTPS URL: '{url}'");
        if (string.IsNullOrWhiteSpace(config.Browser) || config.TimeoutSeconds <= 0 || config.WindowWidth <= 0 || config.WindowHeight <= 0)
            throw new InvalidDataException("Browser, positive timeout and positive window dimensions are required");
        return config;
    }
}
