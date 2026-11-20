"""Static and evidence checks for Lab 6 (standard library only).

Run:
    python Checks/check_lab6.py
    python Checks/check_lab6.py --require-evidence
"""
from pathlib import Path
import re
import sys
import zipfile
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
tests_dir = root / "SauceDemo.Tests/Tests"
pages_dir = root / "SauceDemo.Tests/Pages"
core_dir = root / "SauceDemo.Tests/Core"
utils_dir = root / "SauceDemo.Tests/Utilities"
data_dir = root / "SauceDemo.Tests/Data"

# 1. Preserve Lab 4 POM isolation & forbid logging inside test files
test_files = list(tests_dir.glob("*.cs"))
assert len(test_files) >= 5, f"Expected split test files + AssemblyInfo, found {[p.name for p in test_files]}"

categories = set()
for path in test_files:
    src = path.read_text(encoding="utf-8-sig")
    assert "By" not in src, f"Locator 'By' leaked into {path.name}"
    assert not re.search(r"\bLog\s*\(", src) and not re.search(r"\bLog\.(?:Info|Error)\s*\(", src), (
        f"Test file must NOT call Log directly (must come from BaseTest): {path.name}"
    )
    assert not re.search(r"Thread\.Sleep|Task\.Delay|ImplicitWait", src), f"Non-explicit wait in {path.name}"
    for m in re.finditer(r'(?:Category|SetCategory)\(\s*"([^"]+)"\s*\)', src):
        categories.add(m.group(1))

assert len(categories) >= 2 and {"Smoke", "Slow"}.issubset(categories), (
    f"Expected >= 2 categories including Smoke & Slow, found: {sorted(categories)}"
)

for path in pages_dir.glob("*.cs"):
    src = path.read_text(encoding="utf-8-sig")
    assert "Assert" not in src and "NUnit" not in src, f"Assertion/NUnit in Page: {path.name}"
    assert not re.search(r"public\s+(?:IWebElement|IReadOnlyList<IWebElement>|IEnumerable<IWebElement>)\b", src), (
        f"Public IWebElement API in {path.name}"
    )
    assert not re.search(r"Thread\.Sleep|Task\.Delay|ImplicitWait", src), f"Non-explicit wait in {path.name}"

# 2. Lab 6 structural checks (Log helper, BaseTest, SuiteHooks, Parallel, Exporters, Repository)
for required in [
    data_dir / "AccountRepository.cs",
    utils_dir / "Log.cs",
    utils_dir / "ScreenshotHelper.cs",
    utils_dir / "TestReportCollector.cs",
    utils_dir / "HtmlReportWriter.cs",
    utils_dir / "ExcelReportWriter.cs",
    utils_dir / "PdfReportWriter.cs",
    core_dir / "TestSuiteHooks.cs",
]:
    assert required.exists(), f"Missing Lab 6 component: {required.relative_to(root)}"

log_src = (utils_dir / "Log.cs").read_text(encoding="utf-8-sig")
assert "Info(" in log_src and "Error(" in log_src, "Utilities/Log.cs must implement Info and Error"

base_test_src = (core_dir / "BaseTest.cs").read_text(encoding="utf-8-sig")
assert "Log.Info(" in base_test_src and "Log.Error(" in base_test_src, "BaseTest must log Info and Error"
assert "protected void Log(" not in base_test_src, "BaseTest should delegate to Utilities.Log helper"
assert "TestReportCollector.Record(" in base_test_src, "BaseTest must record test run for HTML/Excel/PDF reports"
assert "ScreenshotHelper.Capture(" in base_test_src, "BaseTest must capture screenshot on failure"

asm_src = (tests_dir / "AssemblyInfo.cs").read_text(encoding="utf-8-sig")
assert "ParallelScope.Fixtures" in asm_src and "LevelOfParallelism(" in asm_src, (
    "AssemblyInfo.cs must configure Parallelizable(ParallelScope.Fixtures) and LevelOfParallelism"
)

# 3. Check Lab 6 runtime evidence when present (or required)
require_evidence = "--require-evidence" in sys.argv[1:]
ev_dir = root / "docs/evidence/lab6"
seq_trx = ev_dir / "sequential.trx"
par_trx = ev_dir / "parallel.trx"

if not (ev_dir.exists() and seq_trx.exists() and par_trx.exists()):
    if require_evidence:
        raise AssertionError("Missing Lab 6 execution evidence in docs/evidence/lab6 (run Checks/run_lab6_evidence.ps1)")
    print(f"STATIC PASS: Lab 6 code/POM/Category/Log/Parallel/Exporter checks OK (categories={sorted(categories)}); runtime evidence pending.")
    sys.exit(0)

ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

def load_outcomes(trx_path: Path) -> dict[str, str]:
    tree = ET.parse(trx_path)
    return {
        r.attrib["testName"]: r.attrib["outcome"]
        for r in tree.findall(".//t:UnitTestResult", ns)
    }

seq = load_outcomes(seq_trx)
par = load_outcomes(par_trx)
assert seq == par, f"Parallel outcomes differ from sequential: {seq} vs {par}"
passed_count = sum(1 for v in par.values() if v == "Passed")
failed_count = sum(1 for v in par.values() if v == "Failed")
assert passed_count == 20 and failed_count == 0, f"Expected 20 Passed, 0 Failed; got {passed_count} Passed, {failed_count} Failed"

log_pf = (ev_dir / "log-pass-fail.log").read_text(encoding="utf-8", errors="replace")
assert "[INFO]" in log_pf and "[ERROR]" in log_pf, "log-pass-fail.log must show both [INFO] and [ERROR] lines"

excel_path = ev_dir / "ketqua-baocao.xlsx"
pdf_path = ev_dir / "ketqua-baocao.pdf"
html_path = ev_dir / "ketqua-baocao.html"
assert html_path.exists() and html_path.stat().st_size > 200, "Missing ketqua-baocao.html"
with zipfile.ZipFile(excel_path) as zf:
    assert "xl/workbook.xml" in zf.namelist() and "xl/worksheets/sheet2.xml" in zf.namelist()
assert pdf_path.read_bytes().startswith(b"%PDF-1.4"), "Invalid PDF header in ketqua-baocao.pdf"

print(f"PASS: Lab 6 static checks + evidence verified ({passed_count} Passed sequential & parallel; [INFO]/[ERROR] logs; HTML/Excel/PDF reports).")
