namespace SauceDemo.Tests.Config;

public class TestConfig
{
    public string BaseUrl { get; set; } = "";
    public string LabBaseUrl { get; set; } = "";
    public string Browser { get; set; } = "";
    public string CocCocBinary { get; set; } = "";
    public int TimeoutSeconds { get; set; }
    public bool Headless { get; set; }
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
}
