using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using SauceDemo.Tests.Config;
using SauceDemo.Tests.Core;
using SauceDemo.Tests.Pages;
using SauceDemo.Tests.Tests;
using SauceDemo.Tests.Utilities;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "SauceDemo.Tests")))
            return dir.FullName;
        dir = dir.Parent;
    }
    throw new DirectoryNotFoundException("Could not locate repository root containing SauceDemo.Tests");
}

static Dictionary<string, string> LoadTrxOutcomes(string trxPath)
{
    Check(File.Exists(trxPath), $"Missing TRX file: {trxPath}");
    XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
    var doc = XDocument.Load(trxPath);
    return doc.Descendants(ns + "UnitTestResult")
        .ToDictionary(
            e => (string)e.Attribute("testName")!,
            e => (string)e.Attribute("outcome")!);
}

static void CheckTrxCounts(string trxPath, int expectedPassed, int expectedFailed)
{
    var outcomes = LoadTrxOutcomes(trxPath);
    var passed = outcomes.Values.Count(v => v == "Passed");
    var failed = outcomes.Values.Count(v => v == "Failed");
    Check(passed == expectedPassed && failed == expectedFailed,
        $"{Path.GetFileName(trxPath)}: expected {expectedPassed} Passed / {expectedFailed} Failed, got {passed} Passed / {failed} Failed");
}

var repoRoot = FindRepoRoot();
var requireEvidence = args.Contains("--require-evidence", StringComparer.OrdinalIgnoreCase);

// --- Static POM & Lab 6 checks ---
var testsDir = Path.Combine(repoRoot, "SauceDemo.Tests", "Tests");
var pagesDir = Path.Combine(repoRoot, "SauceDemo.Tests", "Pages");
var coreDir = Path.Combine(repoRoot, "SauceDemo.Tests", "Core");
var utilsDir = Path.Combine(repoRoot, "SauceDemo.Tests", "Utilities");
var dataDir = Path.Combine(repoRoot, "SauceDemo.Tests", "Data");

var testFiles = Directory.GetFiles(testsDir, "*.cs");
Check(testFiles.Length >= 5, $"Expected split test files + AssemblyInfo, found {testFiles.Length}");
Check(!testFiles.Any(f => Path.GetFileName(f).Equals("UiTests.cs", StringComparison.OrdinalIgnoreCase)),
    "Obsolete UiTests.cs must be removed after splitting fixtures");

foreach (var path in testFiles)
{
    var src = File.ReadAllText(path);
    var fileName = Path.GetFileName(path);
    Check(!src.Contains("By"), $"Locator 'By' leaked into {fileName}");
    Check(!Regex.IsMatch(src, @"\bLog\s*\(") && !Regex.IsMatch(src, @"\bLog\.(?:Info|Error)\s*\("),
        $"Test file must NOT call Log directly (must come from BaseTest): {fileName}");
    Check(!Regex.IsMatch(src, @"Thread\.Sleep|Task\.Delay|ImplicitWait"),
        $"Non-explicit wait in {fileName}");
}

foreach (var path in Directory.GetFiles(pagesDir, "*.cs"))
{
    var src = File.ReadAllText(path);
    var fileName = Path.GetFileName(path);
    Check(!src.Contains("Assert") && !src.Contains("NUnit"), $"Assertion/NUnit in Page: {fileName}");
    Check(!Regex.IsMatch(src, @"public\s+(?:IWebElement|IReadOnlyList<IWebElement>|IEnumerable<IWebElement>)\b"),
        $"Public IWebElement API in {fileName}");
    Check(!Regex.IsMatch(src, @"Thread\.Sleep|Task\.Delay|ImplicitWait"),
        $"Non-explicit wait in {fileName}");
}

foreach (var rel in new[]
{
    "Data/AccountRepository.cs",
    "Utilities/Log.cs",
    "Utilities/ScreenshotHelper.cs",
    "Utilities/TestReportCollector.cs",
    "Utilities/HtmlReportWriter.cs",
    "Utilities/ExcelReportWriter.cs",
    "Utilities/PdfReportWriter.cs",
    "Core/TestSuiteHooks.cs",
    "Tests/AssemblyInfo.cs"
})
{
    Check(File.Exists(Path.Combine(repoRoot, "SauceDemo.Tests", rel)), $"Missing Lab 6 component: {rel}");
}

var baseTestSrc = File.ReadAllText(Path.Combine(coreDir, "BaseTest.cs"));
Check(baseTestSrc.Contains("Log.Info(") && baseTestSrc.Contains("Log.Error("), "BaseTest must log Info and Error");
Check(!baseTestSrc.Contains("protected void Log("), "BaseTest should delegate to Utilities.Log helper");
Check(baseTestSrc.Contains("TestReportCollector.Record("), "BaseTest must record test runs for reports");
Check(baseTestSrc.Contains("ScreenshotHelper.Capture("), "BaseTest must capture screenshot on failure");

// --- Lab 5 checks (preserved) ---
var config = ConfigReader.Load();
Check(config.WindowWidth > 0 && config.WindowHeight > 0, "Window configuration missing");
var invalid = new TestConfig { Browser = "UnsupportedTypo" };
try { DriverFactory.Create(invalid); throw new Exception("Unsupported browser accepted"); }
catch (ArgumentException e) { Check(e.Message.Contains(invalid.Browser), "Error omits invalid browser"); }

var driver = DispatchProxy.Create<IWebDriver, Stub>();
((Stub)(object)driver).Result = (name) => name == "get_Url" ? config.BaseUrl :
    name == "FindElements" ? new ReadOnlyCollection<IWebElement>([]) : throw new NotSupportedException(name);
Check(!new InventoryPage(driver, 1).DaHienThi(), "Negative login must return false without throwing");

var users = JsonSerializer.Deserialize<LoginCase[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data/users.json")))!;
Check(users.Length >= 5 && users.Any(u => u.ThanhCong) && users.Count(u => !u.ThanhCong && u.Username == "") >= 2,
    "Missing positive, negative or boundary data");
Check(users.All(u => u.Username is not null && u.Password is not null && !string.IsNullOrWhiteSpace(u.GhiChu)), "Invalid data schema");
Check(LoginTests.Cases().Count() == users.Length, "JSON rows must each generate one test");

// --- Lab 6 checks ---
var tempDir = Path.Combine(Path.GetTempPath(), "lab6-checks-" + Guid.NewGuid().ToString("N"));
try
{
    var tempLog = Path.Combine(tempDir, "sample.log");
    var infoLine = Log.Info("Sample info check", tempLog);
    var errorLine = Log.Error("Sample error check", tempLog);
    Check(infoLine.Contains("[INFO]") && DateTime.TryParse(infoLine[..23], out _), "Log.Info must format timestamp and [INFO]");
    Check(errorLine.Contains("[ERROR]") && DateTime.TryParse(errorLine[..23], out _), "Log.Error must format timestamp and [ERROR]");
    Check(File.ReadAllText(tempLog).Contains("[ERROR]"), "Log file must persist error entries");

    var testAsm = typeof(LoginTests).Assembly;
    var parallelAttr = testAsm.GetCustomAttributesData()
        .FirstOrDefault(a => a.AttributeType == typeof(ParallelizableAttribute));
    Check(parallelAttr is not null && (ParallelScope)parallelAttr.ConstructorArguments[0].Value! == ParallelScope.Fixtures,
        "Assembly must configure Parallelizable(ParallelScope.Fixtures)");

    var levelAttr = testAsm.GetCustomAttributesData()
        .FirstOrDefault(a => a.AttributeType == typeof(LevelOfParallelismAttribute));
    Check(levelAttr is not null && (int)levelAttr.ConstructorArguments[0].Value! >= 2,
        "Assembly must configure LevelOfParallelism >= 2");

    var fixtureTypes = new[] { typeof(LoginTests), typeof(InventoryTests), typeof(CartTests), typeof(LabTests) };
    var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var t in fixtureTypes)
    {
        foreach (var cat in t.GetCustomAttributesData().Where(a => a.AttributeType == typeof(CategoryAttribute)))
            categories.Add((string)cat.ConstructorArguments[0].Value!);
        foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            foreach (var cat in m.GetCustomAttributesData().Where(a => a.AttributeType == typeof(CategoryAttribute)))
                categories.Add((string)cat.ConstructorArguments[0].Value!);
    }
    foreach (var tc in LoginTests.Cases())
        foreach (var cat in tc.Properties["Category"].OfType<string>())
            categories.Add(cat);
    Check(categories.Count >= 2 && categories.Contains("Smoke") && categories.Contains("Slow"),
        "Tests must define >= 2 [Category] groups including Smoke and Slow");

    TestReportCollector.Reset();
    TestReportCollector.Record(new TestRunRecord("PageTitle", "LoginTests", "Login, Smoke", "Check title", "", config.BaseUrl, "PASS", 1200, "", "", DateTime.UtcNow));
    TestReportCollector.Record(new TestRunRecord("DynamicLoadingWithoutWait", "LabTests", "Lab, Slow", "No wait fail", "", config.LabBaseUrl, "FAIL", 800, "NoSuchElementException", "DynamicLoadingWithoutWait.png", DateTime.UtcNow));

    var reportDir = Path.Combine(tempDir, "reports");
    var exported = TestReportCollector.ExportAll(reportDir, config.Browser, config.BaseUrl);
    TestReportCollector.Reset();

    Check(File.Exists(exported.HtmlPath) && File.ReadAllText(exported.HtmlPath).Contains("DynamicLoadingWithoutWait"), "HTML report missing or invalid");
    using (var zip = ZipFile.OpenRead(exported.ExcelPath))
    {
        Check(zip.GetEntry("xl/workbook.xml") is not null && zip.GetEntry("xl/worksheets/sheet2.xml") is not null, "Excel .xlsx structure invalid");
    }
    var pdfText = Encoding.ASCII.GetString(File.ReadAllBytes(exported.PdfPath));
    Check(pdfText.StartsWith("%PDF-1.4") && pdfText.Contains("%%EOF"), "PDF report header/footer invalid");
}
finally
{
    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
}

if (requireEvidence)
{
    var evDir = Path.Combine(repoRoot, "docs", "evidence", "lab6");
    Check(Directory.Exists(evDir), $"Missing Lab 6 evidence directory: {evDir}");

    CheckTrxCounts(Path.Combine(evDir, "smoke.trx"), 4, 0);
    CheckTrxCounts(Path.Combine(evDir, "not-slow.trx"), 18, 0);
    CheckTrxCounts(Path.Combine(evDir, "ketqua.trx"), 20, 0);

    var seqTrx = Path.Combine(evDir, "sequential.trx");
    var parTrx = Path.Combine(evDir, "parallel.trx");
    CheckTrxCounts(seqTrx, 20, 0);
    CheckTrxCounts(parTrx, 20, 0);

    var seqOutcomes = LoadTrxOutcomes(seqTrx);
    var parOutcomes = LoadTrxOutcomes(parTrx);
    Check(seqOutcomes.Count == parOutcomes.Count &&
          seqOutcomes.All(kv => parOutcomes.TryGetValue(kv.Key, out var v) && v == kv.Value),
        "Parallel outcomes differ from sequential outcomes");

    var logPassFailPath = Path.Combine(evDir, "log-pass-fail.log");
    Check(File.Exists(logPassFailPath), "Missing log-pass-fail.log");
    var logPassFail = File.ReadAllText(logPassFailPath);
    Check(logPassFail.Contains("[INFO]") && logPassFail.Contains("[ERROR]"),
        "log-pass-fail.log must contain both [INFO] and [ERROR]");
    Check(!logPassFail.Contains("secret_sauce", StringComparison.OrdinalIgnoreCase),
        "log-pass-fail.log must not expose test passwords");

    var ketquaHtml = Path.Combine(evDir, "ketqua.html");
    Check(File.Exists(ketquaHtml) && new FileInfo(ketquaHtml).Length > 200, "Missing or empty ketqua.html");

    var customHtml = Path.Combine(evDir, "ketqua-baocao.html");
    Check(File.Exists(customHtml) && new FileInfo(customHtml).Length > 200, "Missing or empty ketqua-baocao.html");

    var excelPath = Path.Combine(evDir, "ketqua-baocao.xlsx");
    Check(File.Exists(excelPath), "Missing ketqua-baocao.xlsx");
    using (var zip = ZipFile.OpenRead(excelPath))
    {
        Check(zip.GetEntry("xl/workbook.xml") is not null && zip.GetEntry("xl/worksheets/sheet2.xml") is not null,
            "Invalid ketqua-baocao.xlsx structure");
    }

    var pdfPath = Path.Combine(evDir, "ketqua-baocao.pdf");
    Check(File.Exists(pdfPath), "Missing ketqua-baocao.pdf");
    var evPdfText = Encoding.ASCII.GetString(File.ReadAllBytes(pdfPath));
    Check(evPdfText.StartsWith("%PDF-1.4") && evPdfText.Contains("%%EOF"),
        "Invalid ketqua-baocao.pdf header/footer");

    var failShot = Path.Combine(evDir, "screenshots", "DynamicLoadingWithoutWait.png");
    Check(File.Exists(failShot) && new FileInfo(failShot).Length > 1000,
        "Missing or empty failure screenshot DynamicLoadingWithoutWait.png");
}

Console.WriteLine(requireEvidence
    ? $"PASS: Lab 5/6 static checks + evidence verified ({users.Length} data rows -> {users.Length} cases; 20 Passed sequential & parallel; [INFO]/[ERROR]; HTML/XLSX/PDF/PNG)."
    : $"PASS: Lab 5/6 static checks ok ({users.Length} data rows -> {users.Length} cases); Log/Parallel/Category/HTML+Excel+PDF exporters verified.");

public class Stub : DispatchProxy
{
    public Func<string, object?> Result = null!;
    protected override object? Invoke(MethodInfo? method, object?[]? args) => Result(method!.Name);
}
