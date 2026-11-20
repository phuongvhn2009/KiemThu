using System.Globalization;
using System.Text;

namespace SauceDemo.Tests.Utilities;

internal static class PdfReportWriter
{
    private const double PageWidth = 842;
    private const double PageHeight = 595;
    private const int RowsPerPage = 16;

    public static void Write(
        string path,
        IReadOnlyList<TestRunRecord> rows,
        string browser,
        string baseUrl,
        double wallSeconds,
        DateTime finishedAtUtc)
    {
        var total = rows.Count;
        var passed = rows.Count(r => string.Equals(r.Status, "PASS", StringComparison.OrdinalIgnoreCase));
        var failed = total - passed;
        var passRate = total == 0 ? 0.0 : passed * 100.0 / total;

        var totalPages = Math.Max(1, (int)Math.Ceiling(rows.Count / (double)RowsPerPage));
        var pageStreams = new List<string>(totalPages);
        for (var p = 0; p < totalPages; p++)
        {
            var slice = rows.Skip(p * RowsPerPage).Take(RowsPerPage).ToList();
            pageStreams.Add(BuildPageContent(
                slice, p * RowsPerPage, p + 1, totalPages,
                browser, baseUrl, total, passed, failed, passRate, wallSeconds, finishedAtUtc));
        }

        WritePdfDocument(path, pageStreams);
    }

    private static string BuildPageContent(
        IReadOnlyList<TestRunRecord> slice,
        int startOffset,
        int pageNum,
        int totalPages,
        string browser,
        string baseUrl,
        int total,
        int passed,
        int failed,
        double passRate,
        double wallSeconds,
        DateTime finishedAtUtc)
    {
        var sb = new StringBuilder(8192);

        // Navy top header banner
        sb.AppendLine("0.118 0.227 0.373 rg 0 535 842 60 re f");
        DrawText(sb, "F2", 15, 24, 567, "1 1 1", "Bao cao Ket qua Kiem thu Tu dong - NUnit + Selenium");
        DrawText(sb, "F1", 9, 24, 548, "0.80 0.86 0.94",
            $"SauceDemo & Lab 1-6 | PASS/FAIL, Link URL & Screenshot Loi | UTC: {finishedAtUtc:yyyy-MM-dd HH:mm:ss}");

        // KPI strip
        sb.AppendLine("0.953 0.957 0.965 rg 24 494 794 32 re f");
        sb.AppendLine("0.82 0.84 0.87 RG 0.6 w 24 494 794 32 re S");
        DrawText(sb, "F2", 9, 34, 512, "0.07 0.09 0.15", $"Tong: {total}");
        DrawText(sb, "F2", 9, 95, 512, "0.09 0.40 0.20", $"PASS: {passed}");
        DrawText(sb, "F2", 9, 165, 512, failed > 0 ? "0.60 0.11 0.11" : "0.07 0.09 0.15", $"FAIL: {failed}");
        DrawText(sb, "F2", 9, 232, 512, "0.07 0.09 0.15", $"Ty le: {passRate:F1}%");
        DrawText(sb, "F2", 9, 316, 512, "0.07 0.09 0.15", $"Thoi gian: {wallSeconds:F2}s");
        DrawText(sb, "F1", 9, 420, 512, "0.20 0.25 0.33", $"Browser: {Truncate(browser, 14)}");
        DrawText(sb, "F1", 8.5, 34, 499, "0.28 0.33 0.41", $"BaseUrl: {Truncate(baseUrl, 95)}");

        // Columns: #, Test Name, Category, KQ, Ms, Account, Link URL, Loi / Screenshot
        var colX = new[] { 24d, 48d, 198d, 273d, 315d, 355d, 445d, 615d, 818d };
        var headers = new[] { "#", "Ten ca test", "Category", "KQ", "Ms", "Tai khoan", "Link URL", "Loi / Screenshot" };

        const double tableTop = 482;
        const double headerH = 20;
        const double rowH = 24;

        // Table header
        sb.AppendLine($"0.118 0.227 0.373 rg {colX[0]:F1} {tableTop - headerH:F1} {colX[^1] - colX[0]:F1} {headerH:F1} re f");
        for (var c = 0; c < headers.Length; c++)
            DrawText(sb, "F2", 8.5, colX[c] + 4, tableTop - 13, "1 1 1", headers[c]);

        // Table rows
        for (var i = 0; i < slice.Count; i++)
        {
            var r = slice[i];
            var yTop = tableTop - headerH - i * rowH;
            var yBottom = yTop - rowH;
            var isPass = string.Equals(r.Status, "PASS", StringComparison.OrdinalIgnoreCase);

            if (i % 2 == 1)
                sb.AppendLine($"0.976 0.980 0.984 rg {colX[0]:F1} {yBottom:F1} {colX[^1] - colX[0]:F1} {rowH:F1} re f");

            // Status cell highlight
            var badgeBg = isPass ? "0.863 0.988 0.906" : "0.996 0.886 0.886";
            sb.AppendLine($"{badgeBg} rg {colX[3]:F1} {yBottom:F1} {colX[4] - colX[3]:F1} {rowH:F1} re f");

            // Borders
            sb.AppendLine($"0.82 0.84 0.87 RG 0.5 w {colX[0]:F1} {yBottom:F1} {colX[^1] - colX[0]:F1} {rowH:F1} re S");

            var yLine1 = yTop - 10;
            var yLine2 = yTop - 20;
            DrawText(sb, "F1", 7.8, colX[0] + 4, yLine1, "0.07 0.09 0.15", (startOffset + i + 1).ToString(CultureInfo.InvariantCulture));
            DrawText(sb, "F1", 7.6, colX[1] + 4, yLine1, "0.07 0.09 0.15", Truncate(r.TestName, 31));
            DrawText(sb, "F1", 7.2, colX[1] + 4, yLine2, "0.40 0.45 0.52", Truncate(r.Scenario, 33));
            DrawText(sb, "F1", 7.5, colX[2] + 4, yLine1, "0.20 0.25 0.33", Truncate(string.IsNullOrWhiteSpace(r.Categories) ? "-" : r.Categories, 15));
            DrawText(sb, "F2", 7.8, colX[3] + 8, yLine1, isPass ? "0.086 0.396 0.204" : "0.600 0.106 0.106", isPass ? "PASS" : "FAIL");
            DrawText(sb, "F1", 7.6, colX[4] + 4, yLine1, "0.07 0.09 0.15", r.DurationMs.ToString(CultureInfo.InvariantCulture));
            DrawText(sb, "F1", 7.5, colX[5] + 4, yLine1, "0.20 0.25 0.33", Truncate(string.IsNullOrWhiteSpace(r.Account) ? "-" : r.Account, 18));
            DrawText(sb, "F1", 7.2, colX[6] + 4, yLine1, "0.08 0.32 0.62", Truncate(string.IsNullOrWhiteSpace(r.Url) ? "-" : r.Url, 44));

            var note1 = string.IsNullOrWhiteSpace(r.ErrorMessage) ? "-" : r.ErrorMessage;
            var note2 = string.IsNullOrWhiteSpace(r.ScreenshotPath) ? "" : "Shot: " + Path.GetFileName(r.ScreenshotPath);
            DrawText(sb, "F1", 7.2, colX[7] + 4, yLine1, string.IsNullOrWhiteSpace(r.ErrorMessage) ? "0.40 0.45 0.52" : "0.60 0.11 0.11", Truncate(note1, 44));
            if (!string.IsNullOrWhiteSpace(note2))
                DrawText(sb, "F1", 7.0, colX[7] + 4, yLine2, "0.28 0.33 0.41", Truncate(note2, 44));
        }

        // Vertical column separators for drawn rows
        var drawnBottom = tableTop - headerH - slice.Count * rowH;
        for (var c = 1; c < colX.Length - 1; c++)
            sb.AppendLine($"0.82 0.84 0.87 RG 0.4 w {colX[c]:F1} {drawnBottom:F1} m {colX[c]:F1} {tableTop:F1} l S");

        // Footer
        DrawText(sb, "F1", 8, 24, 18, "0.40 0.45 0.52", "KiemThu / SauceDemo.Tests - Xuat tu dong tu NUnit TestReportCollector");
        DrawText(sb, "F1", 8, PageWidth - 88, 18, "0.40 0.45 0.52", $"Trang {pageNum} / {totalPages}");

        return sb.ToString();
    }

    private static void DrawText(StringBuilder sb, string fontKey, double size, double x, double y, string rgb, string text)
    {
        sb.AppendLine($"BT /{fontKey} {size:F1} Tf {rgb} rg {x:F1} {y:F1} Td ({ToPdfAscii(text)}) Tj ET");
    }

    private static void WritePdfDocument(string path, IReadOnlyList<string> pageStreams)
    {
        var pageCount = pageStreams.Count;
        const int catalogId = 1;
        const int pagesId = 2;
        var font1Id = 3;
        var font2Id = 4;
        var firstPageId = 5;

        var objects = new Dictionary<int, byte[]>();
        objects[catalogId] = Encoding.ASCII.GetBytes($"<< /Type /Catalog /Pages {pagesId} 0 R >>");

        var kids = string.Join(" ", Enumerable.Range(0, pageCount).Select(i => $"{firstPageId + i * 2} 0 R"));
        objects[pagesId] = Encoding.ASCII.GetBytes($"<< /Type /Pages /Kids [{kids}] /Count {pageCount} >>");
        objects[font1Id] = Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        objects[font2Id] = Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");

        for (var i = 0; i < pageCount; i++)
        {
            var pageId = firstPageId + i * 2;
            var contentId = pageId + 1;
            var contentBytes = Encoding.ASCII.GetBytes(pageStreams[i]);

            objects[pageId] = Encoding.ASCII.GetBytes(
                $"<< /Type /Page /Parent {pagesId} 0 R /MediaBox [0 0 {PageWidth:F0} {PageHeight:F0}] " +
                $"/Resources << /Font << /F1 {font1Id} 0 R /F2 {font2Id} 0 R >> >> /Contents {contentId} 0 R >>");

            using var cs = new MemoryStream();
            var header = Encoding.ASCII.GetBytes($"<< /Length {contentBytes.Length} >>\nstream\n");
            var footer = Encoding.ASCII.GetBytes("\nendstream");
            cs.Write(header);
            cs.Write(contentBytes);
            cs.Write(footer);
            objects[contentId] = cs.ToArray();
        }

        var maxObj = objects.Keys.Max();
        var offsets = new long[maxObj + 1];

        using var ms = new MemoryStream();
        WriteAscii(ms, "%PDF-1.4\n");

        for (var id = 1; id <= maxObj; id++)
        {
            offsets[id] = ms.Position;
            WriteAscii(ms, $"{id} 0 obj\n");
            ms.Write(objects[id]);
            WriteAscii(ms, "\nendobj\n");
        }

        var xrefOffset = ms.Position;
        WriteAscii(ms, $"xref\n0 {maxObj + 1}\n");
        WriteAscii(ms, "0000000000 65535 f \n");
        for (var id = 1; id <= maxObj; id++)
            WriteAscii(ms, $"{offsets[id]:D10} 00000 n \n");

        WriteAscii(ms, $"trailer\n<< /Size {maxObj + 1} /Root {catalogId} 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
        File.WriteAllBytes(path, ms.ToArray());
    }

    private static void WriteAscii(Stream s, string text) => s.Write(Encoding.ASCII.GetBytes(text));

    private static string Truncate(string? value, int maxLen)
    {
        var clean = (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
        return clean.Length <= maxLen ? clean : clean[..Math.Max(0, maxLen - 3)] + "...";
    }

    internal static string ToPdfAscii(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;
            var mapped = ch switch
            {
                'đ' => 'd',
                'Đ' => 'D',
                '\r' or '\n' or '\t' => ' ',
                _ => ch
            };
            if (mapped is '\\' or '(' or ')')
            {
                sb.Append('\\').Append(mapped);
            }
            else if (mapped >= 32 && mapped <= 126)
            {
                sb.Append(mapped);
            }
            else
            {
                sb.Append('?');
            }
        }
        return sb.ToString();
    }
}
