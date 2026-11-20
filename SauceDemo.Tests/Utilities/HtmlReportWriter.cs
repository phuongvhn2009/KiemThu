using System.Net;
using System.Text;

namespace SauceDemo.Tests.Utilities;

internal static class HtmlReportWriter
{
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

        var sb = new StringBuilder(16384);
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"vi\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\" />");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
        sb.AppendLine("  <title>Báo cáo kiểm thử tự động - SauceDemo (NUnit + Selenium)</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    :root { --bg:#0f172a; --card:#1e293b; --line:#334155; --text:#f8fafc; --muted:#94a3b8; --pass-bg:#14532d; --pass-fg:#86efac; --fail-bg:#7f1d1d; --fail-fg:#fca5a5; --accent:#38bdf8; }");
        sb.AppendLine("    * { box-sizing:border-box; }");
        sb.AppendLine("    body { margin:0; padding:24px; font-family:'Segoe UI',system-ui,-apple-system,sans-serif; background:var(--bg); color:var(--text); }");
        sb.AppendLine("    .wrap { max-width:1440px; margin:0 auto; }");
        sb.AppendLine("    h1 { margin:0 0 6px; font-size:24px; }");
        sb.AppendLine("    .sub { color:var(--muted); font-size:13px; margin-bottom:20px; }");
        sb.AppendLine("    .cards { display:grid; grid-template-columns:repeat(auto-fit,minmax(160px,1fr)); gap:12px; margin-bottom:20px; }");
        sb.AppendLine("    .card { background:var(--card); border:1px solid var(--line); border-radius:10px; padding:14px 16px; }");
        sb.AppendLine("    .card .k { font-size:12px; color:var(--muted); text-transform:uppercase; letter-spacing:.04em; }");
        sb.AppendLine("    .card .v { font-size:22px; font-weight:700; margin-top:4px; }");
        sb.AppendLine("    .v.pass { color:var(--pass-fg); } .v.fail { color:var(--fail-fg); }");
        sb.AppendLine("    .toolbar { display:flex; gap:8px; align-items:center; flex-wrap:wrap; margin-bottom:14px; }");
        sb.AppendLine("    .btn { background:var(--card); color:var(--text); border:1px solid var(--line); border-radius:8px; padding:7px 14px; cursor:pointer; font-size:13px; }");
        sb.AppendLine("    .btn.active { border-color:var(--accent); color:var(--accent); }");
        sb.AppendLine("    .tbl-wrap { overflow-x:auto; background:var(--card); border:1px solid var(--line); border-radius:10px; }");
        sb.AppendLine("    table { width:100%; border-collapse:collapse; font-size:13px; }");
        sb.AppendLine("    th, td { padding:10px 12px; border-bottom:1px solid var(--line); text-align:left; vertical-align:top; }");
        sb.AppendLine("    th { background:#0b1324; color:#cbd5e1; position:sticky; top:0; white-space:nowrap; }");
        sb.AppendLine("    tr:hover td { background:rgba(56,189,248,0.06); }");
        sb.AppendLine("    .badge { display:inline-block; padding:2px 9px; border-radius:999px; font-weight:700; font-size:12px; }");
        sb.AppendLine("    .badge.PASS { background:var(--pass-bg); color:var(--pass-fg); }");
        sb.AppendLine("    .badge.FAIL { background:var(--fail-bg); color:var(--fail-fg); }");
        sb.AppendLine("    a { color:var(--accent); text-decoration:none; word-break:break-all; } a:hover { text-decoration:underline; }");
        sb.AppendLine("    .mono { font-family:Consolas,'Courier New',monospace; font-size:12px; }");
        sb.AppendLine("    .err { color:#fca5a5; max-width:320px; white-space:pre-wrap; word-break:break-word; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"wrap\">");
        sb.AppendLine("  <h1>Báo cáo kết quả kiểm thử tự động (NUnit + Selenium)</h1>");
        sb.AppendLine($"  <div class=\"sub\">Trình duyệt: <strong>{E(browser)}</strong> &middot; Base URL: <a href=\"{E(baseUrl)}\" target=\"_blank\" rel=\"noopener\">{E(baseUrl)}</a> &middot; Hoàn tất (UTC): {finishedAtUtc:yyyy-MM-dd HH:mm:ss}</div>");
        sb.AppendLine("  <div class=\"cards\">");
        sb.AppendLine($"    <div class=\"card\"><div class=\"k\">Tổng số ca</div><div class=\"v\">{total}</div></div>");
        sb.AppendLine($"    <div class=\"card\"><div class=\"k\">PASS</div><div class=\"v pass\">{passed}</div></div>");
        sb.AppendLine($"    <div class=\"card\"><div class=\"k\">FAIL</div><div class=\"v fail\">{failed}</div></div>");
        sb.AppendLine($"    <div class=\"card\"><div class=\"k\">Tỷ lệ đạt</div><div class=\"v\">{passRate:F1}%</div></div>");
        sb.AppendLine($"    <div class=\"card\"><div class=\"k\">Tổng thời gian</div><div class=\"v\">{wallSeconds:F2}s</div></div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("  <div class=\"toolbar\">");
        sb.AppendLine($"    <button class=\"btn active\" onclick=\"filterRows('ALL', this)\">Tất cả ({total})</button>");
        sb.AppendLine($"    <button class=\"btn\" onclick=\"filterRows('PASS', this)\">PASS ({passed})</button>");
        sb.AppendLine($"    <button class=\"btn\" onclick=\"filterRows('FAIL', this)\">FAIL ({failed})</button>");
        sb.AppendLine("  </div>");
        sb.AppendLine("  <div class=\"tbl-wrap\">");
        sb.AppendLine("    <table>");
        sb.AppendLine("      <thead><tr>");
        sb.AppendLine("        <th>STT</th><th>Tên ca test</th><th>Fixture</th><th>Category</th><th>Kịch bản kiểm thử</th><th>Tài khoản</th><th>Link URL</th><th>Kết quả</th><th>Thời gian (ms)</th><th>Thông báo lỗi</th><th>Screenshot lỗi</th>");
        sb.AppendLine("      </tr></thead>");
        sb.AppendLine("      <tbody>");

        for (var i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            var statusNorm = string.Equals(r.Status, "PASS", StringComparison.OrdinalIgnoreCase) ? "PASS" : "FAIL";
            var urlCell = IsHttpUrl(r.Url)
                ? $"<a href=\"{E(r.Url)}\" target=\"_blank\" rel=\"noopener\">{E(r.Url)}</a>"
                : E(string.IsNullOrWhiteSpace(r.Url) ? "-" : r.Url);
            var shotCell = "-";
            if (!string.IsNullOrWhiteSpace(r.ScreenshotPath))
            {
                if (File.Exists(r.ScreenshotPath))
                {
                    var uri = new Uri(Path.GetFullPath(r.ScreenshotPath)).AbsoluteUri;
                    shotCell = $"<a class=\"mono\" href=\"{E(uri)}\" target=\"_blank\" rel=\"noopener\">{E(Path.GetFileName(r.ScreenshotPath))}</a>";
                }
                else
                {
                    shotCell = $"<span class=\"mono\">{E(r.ScreenshotPath)}</span>";
                }
            }

            sb.AppendLine($"      <tr data-status=\"{statusNorm}\">");
            sb.AppendLine($"        <td>{i + 1}</td>");
            sb.AppendLine($"        <td class=\"mono\">{E(r.TestName)}</td>");
            sb.AppendLine($"        <td>{E(r.FixtureName)}</td>");
            sb.AppendLine($"        <td>{E(string.IsNullOrWhiteSpace(r.Categories) ? "-" : r.Categories)}</td>");
            sb.AppendLine($"        <td>{E(string.IsNullOrWhiteSpace(r.Scenario) ? "-" : r.Scenario)}</td>");
            sb.AppendLine($"        <td class=\"mono\">{E(string.IsNullOrWhiteSpace(r.Account) ? "-" : r.Account)}</td>");
            sb.AppendLine($"        <td class=\"mono\">{urlCell}</td>");
            sb.AppendLine($"        <td><span class=\"badge {statusNorm}\">{statusNorm}</span></td>");
            sb.AppendLine($"        <td>{r.DurationMs}</td>");
            sb.AppendLine($"        <td class=\"err\">{E(string.IsNullOrWhiteSpace(r.ErrorMessage) ? "-" : r.ErrorMessage)}</td>");
            sb.AppendLine($"        <td>{shotCell}</td>");
            sb.AppendLine("      </tr>");
        }

        sb.AppendLine("      </tbody>");
        sb.AppendLine("    </table>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</div>");
        sb.AppendLine("<script>");
        sb.AppendLine("function filterRows(status, btn) {");
        sb.AppendLine("  document.querySelectorAll('.toolbar .btn').forEach(b => b.classList.remove('active'));");
        sb.AppendLine("  btn.classList.add('active');");
        sb.AppendLine("  document.querySelectorAll('tbody tr').forEach(tr => {");
        sb.AppendLine("    tr.style.display = (status === 'ALL' || tr.dataset.status === status) ? '' : 'none';");
        sb.AppendLine("  });");
        sb.AppendLine("}");
        sb.AppendLine("</script>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    private static string E(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static bool IsHttpUrl(string? url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
