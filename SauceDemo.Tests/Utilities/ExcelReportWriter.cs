using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SauceDemo.Tests.Utilities;

internal static class ExcelReportWriter
{
    private static readonly XNamespace MainNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PackageRelNs = "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly XNamespace ContentTypesNs = "http://schemas.openxmlformats.org/package/2006/content-types";

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
        var passRate = total == 0 ? 0.0 : Math.Round(passed * 100.0 / total, 2);

        if (File.Exists(path)) File.Delete(path);
        using var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        using var zip = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: false);

        WriteXmlEntry(zip, "[Content_Types].xml", BuildContentTypes());
        WriteXmlEntry(zip, "_rels/.rels", BuildPackageRels());
        WriteXmlEntry(zip, "xl/workbook.xml", BuildWorkbook());
        WriteXmlEntry(zip, "xl/_rels/workbook.xml.rels", BuildWorkbookRels());
        WriteXmlEntry(zip, "xl/styles.xml", BuildStyles());
        WriteXmlEntry(zip, "xl/worksheets/sheet1.xml", BuildSummarySheet(browser, baseUrl, total, passed, failed, passRate, wallSeconds, finishedAtUtc));
        WriteXmlEntry(zip, "xl/worksheets/sheet2.xml", BuildDetailSheet(rows));
    }

    private static XDocument BuildContentTypes() => new(
        new XDeclaration("1.0", "utf-8", null),
        new XElement(ContentTypesNs + "Types",
            new XElement(ContentTypesNs + "Default",
                new XAttribute("Extension", "rels"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
            new XElement(ContentTypesNs + "Default",
                new XAttribute("Extension", "xml"),
                new XAttribute("ContentType", "application/xml")),
            new XElement(ContentTypesNs + "Override",
                new XAttribute("PartName", "/xl/workbook.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
            new XElement(ContentTypesNs + "Override",
                new XAttribute("PartName", "/xl/styles.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml")),
            new XElement(ContentTypesNs + "Override",
                new XAttribute("PartName", "/xl/worksheets/sheet1.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
            new XElement(ContentTypesNs + "Override",
                new XAttribute("PartName", "/xl/worksheets/sheet2.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"))
        ));

    private static XDocument BuildPackageRels() => new(
        new XDeclaration("1.0", "utf-8", null),
        new XElement(PackageRelNs + "Relationships",
            new XElement(PackageRelNs + "Relationship",
                new XAttribute("Id", "rId1"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                new XAttribute("Target", "xl/workbook.xml"))
        ));

    private static XDocument BuildWorkbook() => new(
        new XDeclaration("1.0", "utf-8", null),
        new XElement(MainNs + "workbook",
            new XAttribute(XNamespace.Xmlns + "r", RelNs.NamespaceName),
            new XElement(MainNs + "sheets",
                new XElement(MainNs + "sheet",
                    new XAttribute("name", "TongQuan"),
                    new XAttribute("sheetId", "1"),
                    new XAttribute(RelNs + "id", "rId1")),
                new XElement(MainNs + "sheet",
                    new XAttribute("name", "KetQuaChiTiet"),
                    new XAttribute("sheetId", "2"),
                    new XAttribute(RelNs + "id", "rId2"))
            )
        ));

    private static XDocument BuildWorkbookRels() => new(
        new XDeclaration("1.0", "utf-8", null),
        new XElement(PackageRelNs + "Relationships",
            new XElement(PackageRelNs + "Relationship",
                new XAttribute("Id", "rId1"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                new XAttribute("Target", "worksheets/sheet1.xml")),
            new XElement(PackageRelNs + "Relationship",
                new XAttribute("Id", "rId2"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                new XAttribute("Target", "worksheets/sheet2.xml")),
            new XElement(PackageRelNs + "Relationship",
                new XAttribute("Id", "rId3"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                new XAttribute("Target", "styles.xml"))
        ));

    private static XDocument BuildStyles() => new(
        new XDeclaration("1.0", "utf-8", null),
        new XElement(MainNs + "styleSheet",
            new XElement(MainNs + "fonts", new XAttribute("count", "5"),
                Font("11", "FF111827", bold: false),
                Font("11", "FFFFFFFF", bold: true),
                Font("11", "FF166534", bold: true),
                Font("11", "FF991B1B", bold: true),
                Font("11", "FF111827", bold: true)
            ),
            new XElement(MainNs + "fills", new XAttribute("count", "6"),
                new XElement(MainNs + "fill", new XElement(MainNs + "patternFill", new XAttribute("patternType", "none"))),
                new XElement(MainNs + "fill", new XElement(MainNs + "patternFill", new XAttribute("patternType", "gray125"))),
                SolidFill("FF1E3A5F"),
                SolidFill("FFDCFCE7"),
                SolidFill("FFFEE2E2"),
                SolidFill("FFF3F4F6")
            ),
            new XElement(MainNs + "borders", new XAttribute("count", "2"),
                new XElement(MainNs + "border"),
                ThinBorder("FFD1D5DB")
            ),
            new XElement(MainNs + "cellStyleXfs", new XAttribute("count", "1"),
                new XElement(MainNs + "xf",
                    new XAttribute("numFmtId", "0"),
                    new XAttribute("fontId", "0"),
                    new XAttribute("fillId", "0"),
                    new XAttribute("borderId", "0"))
            ),
            new XElement(MainNs + "cellXfs", new XAttribute("count", "5"),
                CellXf(0, 0, 1, null),
                CellXf(1, 2, 1, "center"),
                CellXf(2, 3, 1, "center"),
                CellXf(3, 4, 1, "center"),
                CellXf(4, 5, 1, null)
            ),
            new XElement(MainNs + "cellStyles", new XAttribute("count", "1"),
                new XElement(MainNs + "cellStyle",
                    new XAttribute("name", "Normal"),
                    new XAttribute("xfId", "0"),
                    new XAttribute("builtinId", "0"))
            )
        ));

    private static XElement Font(string size, string rgb, bool bold)
    {
        var el = new XElement(MainNs + "font");
        if (bold) el.Add(new XElement(MainNs + "b"));
        el.Add(new XElement(MainNs + "sz", new XAttribute("val", size)));
        el.Add(new XElement(MainNs + "color", new XAttribute("rgb", rgb)));
        el.Add(new XElement(MainNs + "name", new XAttribute("val", "Calibri")));
        return el;
    }

    private static XElement SolidFill(string rgb) =>
        new(MainNs + "fill",
            new XElement(MainNs + "patternFill", new XAttribute("patternType", "solid"),
                new XElement(MainNs + "fgColor", new XAttribute("rgb", rgb))));

    private static XElement ThinBorder(string rgb) =>
        new(MainNs + "border",
            BorderSide("left", rgb),
            BorderSide("right", rgb),
            BorderSide("top", rgb),
            BorderSide("bottom", rgb),
            new XElement(MainNs + "diagonal"));

    private static XElement BorderSide(string side, string rgb) =>
        new(MainNs + side, new XAttribute("style", "thin"),
            new XElement(MainNs + "color", new XAttribute("rgb", rgb)));

    private static XElement CellXf(int fontId, int fillId, int borderId, string? align)
    {
        var xf = new XElement(MainNs + "xf",
            new XAttribute("numFmtId", "0"),
            new XAttribute("fontId", fontId),
            new XAttribute("fillId", fillId),
            new XAttribute("borderId", borderId),
            new XAttribute("xfId", "0"),
            new XAttribute("applyBorder", "1"));
        if (fontId > 0) xf.Add(new XAttribute("applyFont", "1"));
        if (fillId > 0) xf.Add(new XAttribute("applyFill", "1"));
        xf.Add(new XAttribute("applyAlignment", "1"));
        var alignment = new XElement(MainNs + "alignment",
            new XAttribute("vertical", "center"),
            new XAttribute("wrapText", "1"));
        if (!string.IsNullOrWhiteSpace(align)) alignment.Add(new XAttribute("horizontal", align));
        xf.Add(alignment);
        return xf;
    }

    private static XDocument BuildSummarySheet(
        string browser,
        string baseUrl,
        int total,
        int passed,
        int failed,
        double passRate,
        double wallSeconds,
        DateTime finishedAtUtc)
    {
        var summaryRows = new (string Label, object Value, int ValueStyle)[]
        {
            ("Báo cáo kết quả kiểm thử tự động", "NUnit + Selenium WebDriver", 1),
            ("Trình duyệt", browser, 0),
            ("Link trang đích (BaseUrl)", baseUrl, 0),
            ("Thời điểm xuất (UTC)", finishedAtUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), 0),
            ("Tổng số ca kiểm thử", total, 0),
            ("Số ca PASS", passed, 2),
            ("Số ca FAIL", failed, failed > 0 ? 3 : 0),
            ("Tỷ lệ PASS (%)", Math.Round(passRate, 2), 0),
            ("Tổng thời gian chạy (giây)", Math.Round(wallSeconds, 2), 0)
        };

        var sheetData = new XElement(MainNs + "sheetData");
        sheetData.Add(BuildRow(1,
            TextCell(1, 1, "Thông số", 1),
            TextCell(2, 1, "Giá trị", 1)));

        for (var i = 0; i < summaryRows.Length; i++)
        {
            var rIndex = i + 2;
            var item = summaryRows[i];
            var valueCell = item.Value switch
            {
                int n => NumberCell(2, rIndex, n, item.ValueStyle),
                double d => NumberCell(2, rIndex, d, item.ValueStyle),
                _ => TextCell(2, rIndex, item.Value?.ToString() ?? "", item.ValueStyle)
            };
            sheetData.Add(BuildRow(rIndex,
                TextCell(1, rIndex, item.Label, 4),
                valueCell));
        }

        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(MainNs + "worksheet",
                BuildCols(36, 54),
                sheetData));
    }

    private static XDocument BuildDetailSheet(IReadOnlyList<TestRunRecord> rows)
    {
        var headers = new[]
        {
            "STT", "TenCaTest", "Fixture", "Category", "KichBanKiemThu",
            "TaiKhoan", "LinkURL", "KetQua", "ThoiGianMs", "ThongBaoLoi", "ScreenshotLoi"
        };

        var sheetData = new XElement(MainNs + "sheetData");
        var headerCells = headers.Select((h, idx) => TextCell(idx + 1, 1, h, 1)).ToArray();
        sheetData.Add(BuildRow(1, headerCells));

        for (var i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            var rowIndex = i + 2;
            var isPass = string.Equals(r.Status, "PASS", StringComparison.OrdinalIgnoreCase);
            var statusStyle = isPass ? 2 : 3;

            sheetData.Add(BuildRow(rowIndex,
                NumberCell(1, rowIndex, i + 1, 0),
                TextCell(2, rowIndex, r.TestName, 0),
                TextCell(3, rowIndex, r.FixtureName, 0),
                TextCell(4, rowIndex, r.Categories, 0),
                TextCell(5, rowIndex, r.Scenario, 0),
                TextCell(6, rowIndex, r.Account, 0),
                TextCell(7, rowIndex, r.Url, 0),
                TextCell(8, rowIndex, isPass ? "PASS" : "FAIL", statusStyle),
                NumberCell(9, rowIndex, r.DurationMs, 0),
                TextCell(10, rowIndex, r.ErrorMessage, 0),
                TextCell(11, rowIndex, r.ScreenshotPath, 0)
            ));
        }

        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(MainNs + "worksheet",
                BuildCols(8, 34, 18, 22, 42, 22, 42, 12, 14, 44, 46),
                sheetData));
    }

    private static XElement BuildCols(params double[] widths)
    {
        var cols = new XElement(MainNs + "cols");
        for (var i = 0; i < widths.Length; i++)
        {
            cols.Add(new XElement(MainNs + "col",
                new XAttribute("min", i + 1),
                new XAttribute("max", i + 1),
                new XAttribute("width", widths[i].ToString(CultureInfo.InvariantCulture)),
                new XAttribute("customWidth", "1")));
        }
        return cols;
    }

    private static XElement BuildRow(int rowIndex, params XElement[] cells) =>
        new(MainNs + "row", new XAttribute("r", rowIndex), cells);

    private static XElement TextCell(int colIndex, int rowIndex, string? value, int styleIndex) =>
        new(MainNs + "c",
            new XAttribute("r", CellRef(colIndex, rowIndex)),
            new XAttribute("s", styleIndex),
            new XAttribute("t", "inlineStr"),
            new XElement(MainNs + "is",
                new XElement(MainNs + "t",
                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                    SanitizeXml(value))));

    private static XElement NumberCell(int colIndex, int rowIndex, long value, int styleIndex) =>
        new(MainNs + "c",
            new XAttribute("r", CellRef(colIndex, rowIndex)),
            new XAttribute("s", styleIndex),
            new XAttribute("t", "n"),
            new XElement(MainNs + "v", value.ToString(CultureInfo.InvariantCulture)));

    private static XElement NumberCell(int colIndex, int rowIndex, double value, int styleIndex) =>
        new(MainNs + "c",
            new XAttribute("r", CellRef(colIndex, rowIndex)),
            new XAttribute("s", styleIndex),
            new XAttribute("t", "n"),
            new XElement(MainNs + "v", value.ToString("G", CultureInfo.InvariantCulture)));

    private static string CellRef(int colIndex, int rowIndex)
    {
        var col = "";
        var n = colIndex;
        while (n > 0)
        {
            n--;
            col = (char)('A' + n % 26) + col;
            n /= 26;
        }
        return col + rowIndex.ToString(CultureInfo.InvariantCulture);
    }

    private static string SanitizeXml(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (ch >= 0x20 || ch == '\t' || ch == '\n' || ch == '\r')
                sb.Append(ch);
        }
        return sb.ToString();
    }

    private static void WriteXmlEntry(ZipArchive zip, string entryPath, XDocument doc)
    {
        var entry = zip.CreateEntry(entryPath, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = false
        });
        doc.Save(writer);
    }
}
