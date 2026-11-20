# Lab 6 — Test Runner, báo cáo, logging & chạy song song

Mở rộng trực tiếp project chung [SauceDemo.Tests](../SauceDemo.Tests/SauceDemo.Tests.csproj) trên nền tảng Lab 1–5 (không thêm Checkout, không cài thêm NuGet ngoài BCL .NET 10, giữ nguyên quy ước POM Lab 4 và dữ liệu `users.json` Lab 5).

## 1. Đối chiếu nhận xét của thầy cô (bảng tổng kết trên lớp)

| Điểm nhận xét | Cách xử lý trong framework | File liên quan |
|---|---|---|
| **1. `NUnit.Framework` → `LoginTests.cs` → Code xuất báo cáo Pass/Fail (`PDF`, `Excel`, `Screenshot lỗi`)** | Tách các test fixture rõ ràng (`LoginTests`, `InventoryTests`, `CartTests`, `LabTests`); thêm `TestReportCollector` (thread-safe với `ConcurrentBag`) + `TestSuiteHooks` (`[SetUpFixture]`) tự động xuất `ketqua-baocao.html`, `ketqua-baocao.xlsx` và `ketqua-baocao.pdf` sau mỗi lần chạy; ảnh lỗi chụp qua `ScreenshotHelper` đính kèm NUnit và gắn tên/đường dẫn vào báo cáo. | [LoginTests.cs](../SauceDemo.Tests/Tests/LoginTests.cs), [TestReportCollector.cs](../SauceDemo.Tests/Utilities/TestReportCollector.cs), [ExcelReportWriter.cs](../SauceDemo.Tests/Utilities/ExcelReportWriter.cs), [PdfReportWriter.cs](../SauceDemo.Tests/Utilities/PdfReportWriter.cs), [HtmlReportWriter.cs](../SauceDemo.Tests/Utilities/HtmlReportWriter.cs), [ScreenshotHelper.cs](../SauceDemo.Tests/Utilities/ScreenshotHelper.cs), [TestSuiteHooks.cs](../SauceDemo.Tests/Core/TestSuiteHooks.cs) |
| **2. Kiểm thử các Function (UseCase) → Kịch Bản Kiểm Thử** | Gắn `[Category]` và `[Description]` cho từng UseCase/kịch bản trên cả SauceDemo và `the-internet` (chi tiết trong bảng bên dưới). | [LoginTests.cs](../SauceDemo.Tests/Tests/LoginTests.cs), [InventoryTests.cs](../SauceDemo.Tests/Tests/InventoryTests.cs), [CartTests.cs](../SauceDemo.Tests/Tests/CartTests.cs), [LabTests.cs](../SauceDemo.Tests/Tests/LabTests.cs) |
| **3. Dữ liệu Database (`Login : Account`) → Hiển thị trang Inventory** | Tách lớp truy xuất dữ liệu tài khoản qua `AccountRepository.LoadAll()` đọc `Data/users.json`; ca đăng nhập hợp lệ kiểm tra đủ `DaHienThi()`, tiêu đề `"Products"`, URL `/inventory.html` và đủ 6 sản phẩm trên trang Inventory. | [AccountRepository.cs](../SauceDemo.Tests/Data/AccountRepository.cs), [users.json](../SauceDemo.Tests/Data/users.json), [LoginTests.cs](../SauceDemo.Tests/Tests/LoginTests.cs) |
| **4. Link trang URL** | Đưa `Url => Driver.Url` lên `BasePage` cho mọi Page dùng chung; Assert URL ở Login, Inventory (`/inventory.html`), Cart (`/cart.html`), các trang Lab 3 (`/dynamic_loading/2`, `/javascript_alerts`, `/iframe`), đồng thời ghi URL thực tế vào Log và cột `Link URL` của HTML/Excel/PDF. | [BasePage.cs](../SauceDemo.Tests/Pages/BasePage.cs), [BaseTest.cs](../SauceDemo.Tests/Core/BaseTest.cs) |

## 2. Bảng Kịch Bản Kiểm Thử (UseCase & Category)

| Fixture | Tên ca test | Category | Kịch bản kiểm thử | Nguồn dữ liệu / Input | Link URL đích | Kết quả mong đợi |
|---|---|---|---|---|---|---|
| `LoginTests` | `PageTitle` | `Login`, `Smoke` | Mở trang đăng nhập, kiểm tra tiêu đề & URL gốc | `Config.BaseUrl` | `https://www.saucedemo.com/` | Title = `"Swag Labs"` |
| `LoginTests` | `DangNhap_00_standard_user` | `Login`, `Smoke` | Đăng nhập hợp lệ → hiển thị trang Inventory | `standard_user` (`users.json` #0) | `/inventory.html` | `DaHienThi = true`, `"Products"`, 6 sản phẩm |
| `LoginTests` | `DangNhap_01_performance_glitch_user` | `Login`, `Slow` | Đăng nhập tài khoản phản hồi chậm bằng Explicit Wait | `performance_glitch_user` (`users.json` #1) | `/inventory.html` | `DaHienThi = true`, `"Products"`, 6 sản phẩm |
| `LoginTests` | `DangNhap_02_locked_out_user` … `DangNhap_10_standard_user` (9 ca) | `Login` | Kiểm thử ca khóa, sai mật khẩu, rỗng user/pass, user không tồn tại, khoảng trắng thừa, SQL injection string, mật khẩu 256 ký tự | `users.json` #2–#10 | `https://www.saucedemo.com/` | `DaHienThi = false`, hiển thị đúng thông báo lỗi `Epic sadface` |
| `InventoryTests` | `Products` | `Inventory`, `Smoke` | Đăng nhập, lấy 6 sản phẩm qua `item.FindElement`, kiểm tra tên/giá | `standard_user` | `/inventory.html` | 6 sản phẩm, tên không rỗng, giá `$...`, có `"Sauce Labs Backpack"` |
| `InventoryTests` | `PricesAscending` | `Inventory` | Chaining `DangNhap().SapXep("lohi").LayDanhSachGia()` | `standard_user`, `lohi` | `/inventory.html` | 6 giá số `Is.Ordered.Ascending` |
| `CartTests` | `AddBackpack` | `Cart`, `Smoke` | Thêm Backpack từ Inventory rồi mở giỏ hàng | `standard_user` | `/cart.html` | Tên `"Sauce Labs Backpack"`, số lượng `"1"` |
| `LabTests` | `DynamicLoading` | `Lab`, `Slow` | Bấm Start, dùng Explicit Wait chờ `#finish` xuất hiện | `Config.LabBaseUrl` | `/dynamic_loading/2` | Text = `"Hello World!"`, đính kèm ảnh |
| `LabTests` | `DynamicLoadingWithoutWait` | `Lab`, `Slow`, `Explicit` | Cố ý không chờ `#finish` để chứng minh `NoSuchElementException` & log `[ERROR]` | `Config.LabBaseUrl` | `/dynamic_loading/2` | RỚT dự kiến + ảnh lỗi + `[ERROR]` |
| `LabTests` | `JavaScriptAlert` (3 ca) | `Lab` | Xử lý Alert (Accept), Confirm (Dismiss), Prompt (`Hello Lab3`) | `jsAlert`, `jsConfirm`, `jsPrompt` | `/javascript_alerts` | Đúng text popup & `#result` |
| `LabTests` | `Iframe` | `Lab` | `SwitchTo().Frame(...)` nhập `"Hello Lab3"`, thoát về `DefaultContent()` | `"Hello Lab3"` | `/iframe` | Nội dung editor = `"Hello Lab3"`, Heading chứa `"iFrame"` |

## 3. Đối chiếu 5 phần A–E của Lab 6

### Phần A — Tổ chức test với `[Category]`
- Các nhóm `[Category]` gồm: `Smoke`, `Login`, `Inventory`, `Cart`, `Lab`, `Slow`.
- Chạy chỉ nhóm khói: `dotnet test SauceDemo.Tests --filter "Category=Smoke"`
- Chạy loại trừ nhóm chậm: `dotnet test SauceDemo.Tests --filter "Category!=Slow"`

### Phần B — Logging vào `BaseTest`
- `Utilities/Log.cs` cung cấp `Log.Info(...)` và `Log.Error(...)` in ra `TestContext.Progress.WriteLine` (hiển thị với `-l "console;verbosity=detailed"`) và ghi vào `test.log` theo định dạng `yyyy-MM-dd HH:mm:ss.fff [INFO|ERROR] [T<ThreadId>] ...`.
- Toàn bộ mốc `START` (`[SetUp]`) và `OK`/`ROT` (`[TearDown]`) nằm trong `Core/BaseTest.cs`; **các file trong `Tests/` không gọi `Log` trực tiếp**.
- Không ghi mật khẩu vào log hoặc báo cáo.

### Phần C — Báo cáo HTML, Excel & PDF
- Xuất `.trx` chuẩn NUnit: `dotnet test SauceDemo.Tests --logger "trx;LogFileName=ketqua.trx"` rồi chuyển sang HTML trực quan bằng `trxlog2html`.
- Song song đó, `TestSuiteHooks` tự động xuất 3 định dạng báo cáo trong `<TEST_ARTIFACTS>/reports/`:
  - `ketqua-baocao.html` (có bộ lọc `Tất cả / PASS / FAIL`, link URL và link ảnh lỗi)
  - `ketqua-baocao.xlsx` (OpenXML thuần BCL gồm sheet `TongQuan` & `KetQuaChiTiet`)
  - `ketqua-baocao.pdf` (PDF 1.4 chuẩn bảng A4 ngang kèm trạng thái PASS/FAIL, URL & screenshot lỗi)

### Phần D — Chạy song song & giải thích Thread-Safety
Cấu hình trong `Tests/AssemblyInfo.cs`:
```csharp
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(4)]
```
Framework đảm bảo **thread-safe** khi chạy 4 fixture (`LoginTests`, `InventoryTests`, `CartTests`, `LabTests`) song song vì `Driver`, `Config`, `Login`, `directory`, `logFilePath` và `stopwatch` đều là **instance field** của từng đối tượng test trên `BaseTest` (không dùng biến `static` chia sẻ phiên trình duyệt). Mỗi ca test có một phiên WebDriver độc lập và thư mục artifact riêng theo `Guid`, còn phần gom kết quả cuối suite trong `TestReportCollector` dùng `ConcurrentBag<TestRunRecord>` và lock riêng khi ghi file nên các luồng không ghi đè dữ liệu của nhau.

### Phần E — AI sinh ca kiểm thử & thẩm định (ô nhập Password)

- **Prompt đã dùng:**
  > "Liệt kê các ca kiểm thử (gồm ca hợp lệ, ca âm và ca biên) kèm câu Assert dạng NUnit (`Assert.That(...)`) cho ô nhập Password trên trang đăng nhập `https://www.saucedemo.com/` (tài khoản mẫu `standard_user` / `secret_sauce`). Ghi rõ điều kiện nhập, giá trị mong đợi và câu Assert."

- **Toàn bộ 8 đề xuất AI trả về:**
  1. **TC01 — Mật khẩu đúng (`"secret_sauce"`)**: Đăng nhập với `standard_user` → Kỳ vọng vào trang Inventory (`Assert.That(inventory.DaHienThi(), Is.True); Assert.That(inventory.Url, Does.EndWith("/inventory.html"));`).
  2. **TC02 — Sai mật khẩu (`"wrong_pass"`)**: Đăng nhập với `standard_user` → Kỳ vọng báo lỗi mismatch (`Assert.That(Login.LayThongBaoLoi(), Is.EqualTo("Epic sadface: Username and password do not match any user in this service"));`).
  3. **TC03 — Để trống Password (`""`)**: Đăng nhập với `standard_user` → Kỳ vọng báo lỗi thiếu mật khẩu (`Assert.That(Login.LayThongBaoLoi(), Is.EqualTo("Epic sadface: Password is required"));`).
  4. **TC04 — Biên tối thiểu 1 ký tự (`"a"`)**: Đăng nhập với `standard_user` → Kỳ vọng báo lỗi mismatch (`Assert.That(Login.LayThongBaoLoi(), Is.EqualTo("Epic sadface: Username and password do not match any user in this service"));`).
  5. **TC05 — Biên chuỗi dài 256 ký tự (`new string('x', 256)`)**: Đăng nhập với `standard_user` → Kỳ vọng không vào được Inventory (`Assert.That(inventory.DaHienThi(), Is.False);`).
  6. **TC06 — Mật khẩu yếu dưới 8 ký tự, thiếu chữ hoa & số (`"123"`)**: AI kỳ vọng hệ thống báo lỗi chính sách độ mạnh mật khẩu (`Assert.That(Login.LayThongBaoLoi(), Does.Contain("at least 8 characters"));`).
  7. **TC07 — Nhập sai Password 5 lần liên tiếp**: AI kỳ vọng tài khoản tự động bị khóa tạm thời 15 phút (`Assert.That(Login.LayThongBaoLoi(), Does.Contain("temporarily locked for 15 minutes"));`).
  8. **TC08 — Mật khẩu có khoảng trắng đầu/cuối (`" secret_sauce "`)**: AI giả định hệ thống tự `.Trim()` nên vẫn đăng nhập thành công (`Assert.That(inventory.DaHienThi(), Is.True);`).

- **Thẩm định — 3 ca AI đề xuất KHÔNG dùng được (theo đề xuất gốc):**
  1. **TC06 (Chính sách độ mạnh mật khẩu ≥ 8 ký tự / chữ hoa / số):** Không dùng được vì form đăng nhập SauceDemo không có rule kiểm tra độ phức tạp mật khẩu; thực tế nhập `"123"` trả về lỗi sai thông tin đăng nhập (`do not match any user in this service`), không có thông báo `"at least 8 characters"`.
  2. **TC07 (Tự động khóa tạm thời 15 phút sau 5 lần sai):** Không dùng được vì SauceDemo là web demo xác thực tĩnh phía client, không có bộ đếm số lần đăng nhập sai hay khóa động theo thời gian (chỉ có tài khoản khóa cố định `locked_out_user`).
  3. **TC08 (Khoảng trắng đầu/cuối tự trim → `Is.True`):** Không dùng được với kỳ vọng `true` vì SauceDemo đối chiếu nguyên văn chuỗi mật khẩu và từ chối đăng nhập; nếu dùng làm ca kiểm thử phải sửa kỳ vọng thành `Is.False` và kiểm tra lỗi mismatch.

## 4. Kết quả thực thi & minh chứng thực tế (20/11/2026)

Chạy thực tế trên **Chrome 158.0.8127.36 headless**, **.NET SDK 10.0.403**, **`trxlog2html` 1.0.4** (chi tiết đầy đủ xem tại [hồ sơ thực thi Lab 6](evidence/lab6/README.md)):

### 4.1 Bảng đo đạc & xác minh thực tế

| Hạng mục | Lệnh / Cấu hình | Kết quả | Thời gian thực đo | Minh chứng |
|---|---|---|---|---|
| **Build & Checks** | `dotnet build` + `Checks/Lab5Checks.csproj -- --require-evidence` | **0 warning, 0 error; PASS** | — | [build.log](evidence/lab6/build.log), [checks.log](evidence/lab6/checks.log) |
| **Phần A — Smoke** | `--filter "Category=Smoke"` | **4 / 4 PASS** | **6.11s** | [smoke.trx](evidence/lab6/smoke.trx), [ảnh Smoke](../screenshots/lab6-category-smoke.png) |
| **Phần A — Trừ Slow** | `--filter "Category!=Slow"` | **18 / 18 PASS** | **52.95s** | [not-slow.trx](evidence/lab6/not-slow.trx), [ảnh Category!=Slow](../screenshots/lab6-category-not-slow.png) |
| **Phần B — Log `[INFO]`/`[ERROR]`** | `PageTitle` (`[INFO]`) + `DynamicLoadingWithoutWait` (`[ERROR]`) | **1 PASS + 1 FAIL cố ý** | 1.91s + 5.12s | [log-pass-fail.log](evidence/lab6/log-pass-fail.log), [ảnh log](../screenshots/lab6-log.png), [ảnh lỗi trang web](evidence/lab6/screenshots/DynamicLoadingWithoutWait.png), [pass-fail-baocao.html](evidence/lab6/pass-fail-baocao.html) |
| **Phần C — Báo cáo TRX → HTML / Excel / PDF** | `trxlog2html` + `TestSuiteHooks` | **20 / 20 PASS** | **60.10s** | [ketqua.trx](evidence/lab6/ketqua.trx), [ketqua.html](evidence/lab6/ketqua.html), [ảnh trxlog2html](../screenshots/lab6-html-report.png), [ketqua-baocao.html](evidence/lab6/ketqua-baocao.html), [ảnh báo cáo tự động](../screenshots/lab6-report-custom.png), [ketqua-baocao.xlsx](evidence/lab6/ketqua-baocao.xlsx), [ketqua-baocao.pdf](evidence/lab6/ketqua-baocao.pdf) |
| **Phần D — Tuần tự** | `-- NUnit.NumberOfTestWorkers=0` | **20 / 20 PASS** | **92.88s** | [sequential.trx](evidence/lab6/sequential.trx), [ảnh tuần tự](../screenshots/lab6-sequential.png) |
| **Phần D — Song song** | `ParallelScope.Fixtures`, `LevelOfParallelism(4)` | **20 / 20 PASS** (100% khớp tuần tự) | **60.67s** (**nhanh hơn 32.21s = 34.7% / 1.53x**) | [parallel.trx](evidence/lab6/parallel.trx), [ảnh song song](../screenshots/lab6-parallel.png) |

> **Nhận xét tốc độ song song:** Thời gian song song giảm từ **92.88s xuống 60.67s** (**-34.7%**) và giữ nguyên **20/20 PASS** không phát sinh lỗi tranh chấp tài nguyên. Đường găng hiện là fixture `LoginTests` (12 ca data-driven từ `users.json` chạy tuần tự trong cùng fixture, gồm cả `performance_glitch_user` có độ trễ chủ đích ~5s), trong khi `InventoryTests`, `CartTests` và `LabTests` hoàn tất đồng thời trên các luồng còn lại.

### 4.2 Trích xuất log thật (`docs/evidence/lab6/log-pass-fail.log`)

```text
2026-11-20 21:25:59.015 [INFO] [T15] START PageTitle | Fixture=LoginTests | Browser=Chrome | BaseUrl=https://www.saucedemo.com/
2026-11-20 21:26:00.116 [INFO] [T15] OK PageTitle (1111ms) | URL=https://www.saucedemo.com/
...
2026-11-20 21:26:01.714 [INFO] [T15] START DynamicLoadingWithoutWait | Fixture=LabTests | Browser=Chrome | BaseUrl=https://www.saucedemo.com/
2026-11-20 21:26:06.059 [ERROR] [T15] ROT DynamicLoadingWithoutWait (4280ms) | URL=https://the-internet.herokuapp.com/dynamic_loading/2 | Error=OpenQA.Selenium.NoSuchElementException : no such element: Unable to locate element: {"method":"css selector","selector":"#finish"} (Session info: chrome=158.0.8127.36); For documentation on this error, please visit: https://www.selenium.dev/documentation/webdriver/troubleshooting/errors#nosuchelementexception | Screenshot=D:\KiemThu\TestResults\lab6\artifacts\35813812bc7049ccad4d2a97972b68ce\DynamicLoadingWithoutWait.png
```

### 4.3 Lệnh chạy lại & kiểm chứng đầy đủ

Chạy trọn bộ tự động thu thập bằng chứng Lab 6 trên Windows PowerShell (`powershell.exe`):

```powershell
powershell.exe -ExecutionPolicy Bypass -File Checks/run_lab6_evidence.ps1
```

Hoặc chạy kiểm tra tĩnh + kiểm chứng toàn bộ bằng chứng:

```powershell
dotnet build SauceDemo.Tests
dotnet test SauceDemo.Tests --filter "Category=Smoke"
dotnet test SauceDemo.Tests --filter "Category!=Slow"
dotnet test SauceDemo.Tests --filter "FullyQualifiedName=SauceDemo.Tests.Tests.LoginTests.PageTitle|FullyQualifiedName=SauceDemo.Tests.Tests.LabTests.DynamicLoadingWithoutWait" -l "console;verbosity=detailed"
dotnet test SauceDemo.Tests --filter "FullyQualifiedName=SauceDemo.Tests.Tests.LabTests.DynamicLoadingWithoutWait" -l "console;verbosity=detailed"
dotnet test SauceDemo.Tests --logger "trx;LogFileName=ketqua.trx" --results-directory docs/evidence/lab6
trxlog2html -i docs/evidence/lab6/ketqua.trx -o docs/evidence/lab6/ketqua.html
dotnet test SauceDemo.Tests --logger "trx;LogFileName=sequential.trx" --results-directory docs/evidence/lab6 -- NUnit.NumberOfTestWorkers=0
dotnet test SauceDemo.Tests --logger "trx;LogFileName=parallel.trx" --results-directory docs/evidence/lab6
dotnet run --project Checks/Lab5Checks.csproj -- --require-evidence
```
