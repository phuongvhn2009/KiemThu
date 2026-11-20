using System.Collections.Concurrent;

namespace SauceDemo.Tests.Utilities;

public sealed record TestRunRecord(
    string TestName,
    string FixtureName,
    string Categories,
    string Scenario,
    string Account,
    string Url,
    string Status,
    long DurationMs,
    string ErrorMessage,
    string ScreenshotPath,
    DateTime FinishedAt);

public sealed record ExportedReports(string HtmlPath, string ExcelPath, string PdfPath);

public static class TestReportCollector
{
    private static readonly ConcurrentBag<TestRunRecord> Entries = new();
    private static long startedAtTicks = DateTime.UtcNow.Ticks;

    public static int Count => Entries.Count;

    public static void Reset()
    {
        Entries.Clear();
        Interlocked.Exchange(ref startedAtTicks, DateTime.UtcNow.Ticks);
    }

    public static void Record(TestRunRecord entry) => Entries.Add(entry);

    public static IReadOnlyList<TestRunRecord> Snapshot() =>
        Entries.OrderBy(e => e.FixtureName, StringComparer.Ordinal)
               .ThenBy(e => e.TestName, StringComparer.Ordinal)
               .ToArray();

    public static ExportedReports ExportAll(string outputDirectory, string browser, string baseUrl)
    {
        Directory.CreateDirectory(outputDirectory);
        var rows = Snapshot();
        var finishedAtUtc = DateTime.UtcNow;
        var startedAtUtc = new DateTime(Interlocked.Read(ref startedAtTicks), DateTimeKind.Utc);
        var wallSeconds = Math.Max(0.01, (finishedAtUtc - startedAtUtc).TotalSeconds);

        var htmlPath = Path.Combine(outputDirectory, "ketqua-baocao.html");
        var excelPath = Path.Combine(outputDirectory, "ketqua-baocao.xlsx");
        var pdfPath = Path.Combine(outputDirectory, "ketqua-baocao.pdf");

        HtmlReportWriter.Write(htmlPath, rows, browser, baseUrl, wallSeconds, finishedAtUtc);
        ExcelReportWriter.Write(excelPath, rows, browser, baseUrl, wallSeconds, finishedAtUtc);
        PdfReportWriter.Write(pdfPath, rows, browser, baseUrl, wallSeconds, finishedAtUtc);

        return new ExportedReports(htmlPath, excelPath, pdfPath);
    }
}
