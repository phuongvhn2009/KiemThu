# Lab 6 — Hồ sơ thực thi (phiên 20/11/2026)

Thu thập trực tiếp trên nhánh `feat/lab6-runner-reporting` của project chung [SauceDemo.Tests](../../../SauceDemo.Tests/SauceDemo.Tests.csproj) bằng **Headless Chrome 158.0.8127.36**, **.NET SDK 10.0.403** và **`trxlog2html` 1.0.4**. Không chỉnh sửa minh chứng Lab 4 / Lab 5 lịch sử và không ghi mật khẩu vào log hay báo cáo.

## 1. Bảng kết quả thực thi thật

| Phần | Lượt chạy / Kiểm chứng | Kết quả | Thời gian wall-clock (TRX) | File bằng chứng |
|---|---|---|---|---|
| Build | `dotnet build SauceDemo.Tests` | **0 warning, 0 error** | — | [build.log](build.log) |
| Kiểm tra tĩnh & chứng cứ | `dotnet run --project Checks/Lab5Checks.csproj -- --require-evidence` | **PASS** (11 dòng JSON, POM, Log, Parallel, TRX, HTML/XLSX/PDF, PNG) | — | [checks.log](checks.log) |
| **A** (`Category=Smoke`) | Lọc 4 ca smoke | **4 / 4 PASS** | **6.11s** (`21:24:57.60` → `21:25:03.71`) | [smoke.trx](smoke.trx), [smoke.log](smoke.log) |
| **A** (`Category!=Slow`) | Loại trừ nhóm `Slow` (`performance_glitch_user`, `DynamicLoading`) | **18 / 18 PASS** | **52.95s** (`21:25:04.79` → `21:25:57.74`) | [not-slow.trx](not-slow.trx), [not-slow.log](not-slow.log) |
| **B** (`PageTitle`) | Ca thuận có `[INFO]` từ `BaseTest` | **1 / 1 PASS** | **1.91s** | [log-pass-fail.log](log-pass-fail.log), [log-pass-fail.trx](log-pass-fail.trx) |
| **B** (`DynamicLoadingWithoutWait`) | Ca cố ý lỗi `[Explicit]` để thu `[ERROR]` & ảnh lỗi | **0 PASS / 1 FAIL (dự kiến)** | **5.12s** | [log-fail-explicit.trx](log-fail-explicit.trx), [pass-fail-baocao.html](pass-fail-baocao.html), [DynamicLoadingWithoutWait.png](screenshots/DynamicLoadingWithoutWait.png) |
| **C** (Full suite + `trxlog2html`) | Toàn bộ 20 ca thường lệ (1 Explicit skipped) | **20 / 20 PASS** | **60.10s** | [ketqua.trx](ketqua.trx), [ketqua.html](ketqua.html), [ketqua-baocao.html](ketqua-baocao.html), [ketqua-baocao.xlsx](ketqua-baocao.xlsx), [ketqua-baocao.pdf](ketqua-baocao.pdf) |
| **D** (`Workers=0`) | Chạy **tuần tự** toàn bộ 20 ca | **20 / 20 PASS** | **92.88s** (`21:27:09.18` → `21:28:42.06`) | [sequential.trx](sequential.trx), [sequential.log](sequential.log) |
| **D** (`ParallelScope.Fixtures`, `LOP=4`) | Chạy **song song** toàn bộ 20 ca | **20 / 20 PASS** | **60.67s** (`21:28:42.86` → `21:29:43.53`) | [parallel.trx](parallel.trx), [parallel.log](parallel.log) |

## 2. Lưu ý minh bạch về chạy Explicit & ảnh chụp minh chứng

- Khi lọc kết hợp `PageTitle|DynamicLoadingWithoutWait` theo chế độ non-explicit của NUnit, NUnit chỉ thực thi `PageTitle` (`[INFO]`); ngay sau đó runner chạy tiếp lệnh fallback riêng cho `DynamicLoadingWithoutWait` (chế độ Explicit) và nối kết quả vào cùng file [log-pass-fail.log](log-pass-fail.log), đồng thời lưu báo cáo lỗi tại [pass-fail-baocao.html](pass-fail-baocao.html), [pass-fail-baocao.xlsx](pass-fail-baocao.xlsx), [pass-fail-baocao.pdf](pass-fail-baocao.pdf) và ảnh chụp trang web lỗi tại [screenshots/DynamicLoadingWithoutWait.png](screenshots/DynamicLoadingWithoutWait.png).
- Trong các file `screenshots/lab6-*.png` tại thư mục gốc:
  - [lab6-html-report.png](../../../screenshots/lab6-html-report.png) và [lab6-report-custom.png](../../../screenshots/lab6-report-custom.png) là ảnh chụp **trực tiếp** bằng Chrome headless từ [ketqua.html](ketqua.html) (`trxlog2html`) và [ketqua-baocao.html](ketqua-baocao.html).
  - [lab6-category-smoke.png](../../../screenshots/lab6-category-smoke.png), [lab6-category-not-slow.png](../../../screenshots/lab6-category-not-slow.png), [lab6-log.png](../../../screenshots/lab6-log.png), [lab6-sequential.png](../../../screenshots/lab6-sequential.png) và [lab6-parallel.png](../../../screenshots/lab6-parallel.png) là ảnh render trình duyệt hiển thị **nguyên văn** log CLI và số liệu `.trx` thật (có ghi chú rõ *không phải ảnh chụp Test Explorer GUI*).
