using NUnit.Framework;
using SauceDemo.Tests.Config;
using SauceDemo.Tests.Utilities;

namespace SauceDemo.Tests;

[SetUpFixture]
public class TestSuiteHooks
{
    [OneTimeSetUp]
    public void BeforeSuite()
    {
        TestReportCollector.Reset();
        Log.Info("SUITE START SauceDemo.Tests");
    }

    [OneTimeTearDown]
    public void AfterSuite()
    {
        if (TestReportCollector.Count == 0) return;

        var artifactsRoot = Environment.GetEnvironmentVariable("TEST_ARTIFACTS")
            ?? Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts");
        var reportDir = Path.Combine(artifactsRoot, "reports");

        var browser = "Chrome";
        var baseUrl = "https://www.saucedemo.com/";
        try
        {
            var cfg = ConfigReader.Load();
            browser = cfg.Browser;
            baseUrl = cfg.BaseUrl;
        }
        catch { }

        var exported = TestReportCollector.ExportAll(reportDir, browser, baseUrl);
        Log.Info($"SUITE END | Reports exported: HTML={exported.HtmlPath} | Excel={exported.ExcelPath} | PDF={exported.PdfPath}");
    }
}
