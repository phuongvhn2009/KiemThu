
# SauceDemo - NUnit + Selenium

## Lab 6 — Test Runner, báo cáo, logging & chạy song song

Nâng cấp trực tiếp từ Lab 5 trên project chung `SauceDemo.Tests` (giữ nguyên POM Lab 4, dữ liệu `Data/users.json` Lab 5, không thêm Checkout và không thêm dependency NuGet ngoài). Xem đầy đủ bảng Kịch bản kiểm thử, đối chiếu nhận xét giảng viên và thẩm định AI tại [docs/lab6.md](docs/lab6.md).

- **Nhận xét giảng viên & Tổ chức UseCase (`Tests/`)**: tách thành `LoginTests.cs`, `InventoryTests.cs`, `CartTests.cs`, `LabTests.cs` và `Tests/AssemblyInfo.cs`; tách truy xuất dữ liệu sang `Data/AccountRepository.cs`; kiểm tra chặt hiển thị Inventory (`DaHienThi()`, `"Products"`, `/inventory.html`, 6 sản phẩm) và bổ sung `BasePage.Url` cùng kiểm tra URL ở Login/Inventory/Cart/Lab.
- **Phần A — `[Category]`**: gắn các nhóm `Smoke`, `Login`, `Inventory`, `Cart`, `Lab`, `Slow` để chạy chọn lọc `--filter "Category=Smoke"` hoặc loại trừ `--filter "Category!=Slow"`.
- **Phần B — Logging vào `BaseTest`**: thêm `Utilities/Log.cs` (`Info`, `Error` kèm timestamp và thread id); mọi dòng `START` ở `[SetUp]` và `OK` / `ROT` ở `[TearDown]` xuất phát từ `Core/BaseTest.cs`, **không gọi log trong file test**.
- **Phần C — Báo cáo HTML, Excel, PDF & Screenshot lỗi**: `Utilities/TestReportCollector.cs` + `Core/TestSuiteHooks.cs` (`[SetUpFixture]`) tự động xuất `ketqua-baocao.html`, `ketqua-baocao.xlsx` và `ketqua-baocao.pdf` (gồm trạng thái PASS/FAIL, tài khoản, Link URL và đường dẫn ảnh lỗi từ `ScreenshotHelper`) song song với xuất `ketqua.trx` + `trxlog2html`.
- **Phần D — Chạy song song & Thread-safety**: bật `[assembly: Parallelizable(ParallelScope.Fixtures)]` và `[assembly: LevelOfParallelism(4)]` trên 4 fixture độc lập. Framework thread-safe vì `Driver`, `Config`, `Login`, `logFilePath` và `stopwatch` đều là **instance field** của từng test instance trên `BaseTest` (không có `Driver` `static` dùng chung), mỗi test dùng thư mục `Guid` riêng và `TestReportCollector` thu thập kết quả bằng `ConcurrentBag`.
- **Phần E — AI sinh & thẩm định (ô Password SauceDemo)**: lưu prompt và 8 ca + `Assert.That` do AI đề xuất trong [docs/lab6.md](docs/lab6.md), chỉ ra 3 ca không dùng được (chính sách độ phức tạp >= 8 ký tự, khóa tạm 15 phút sau 5 lần sai, tự trim khoảng trắng đầu/cuối kỳ vọng `true`).

- **Kết quả kiểm chứng thực tế (Chrome 158 headless, 20/11/2026)**: Build **0 warning / 0 error**; `Category=Smoke` **4/4 PASS (6.11s)**; `Category!=Slow` **18/18 PASS (52.95s)**; log `[INFO]` (`PageTitle` PASS) & `[ERROR]` (`DynamicLoadingWithoutWait` FAIL cố ý kèm ảnh lỗi [DynamicLoadingWithoutWait.png](docs/evidence/lab6/screenshots/DynamicLoadingWithoutWait.png)); toàn bộ suite **20/20 PASS** xuất đủ [ketqua.html](docs/evidence/lab6/ketqua.html) (`trxlog2html`) + [ketqua-baocao.html](docs/evidence/lab6/ketqua-baocao.html) + [ketqua-baocao.xlsx](docs/evidence/lab6/ketqua-baocao.xlsx) + [ketqua-baocao.pdf](docs/evidence/lab6/ketqua-baocao.pdf); chạy **tuần tự 92.88s vs song song 60.67s** (**nhanh hơn 32.21s = 34.7% / 1.53x**, kết quả **20/20 PASS** trùng khớp hoàn toàn). Xem [hồ sơ Lab 6](docs/evidence/lab6/README.md) và ảnh minh chứng: [Smoke](screenshots/lab6-category-smoke.png), [Category!=Slow](screenshots/lab6-category-not-slow.png), [Log PASS/FAIL](screenshots/lab6-log.png), [HTML trxlog2html](screenshots/lab6-html-report.png), [Báo cáo PASS/FAIL+URL](screenshots/lab6-report-custom.png), [Tuần tự](screenshots/lab6-sequential.png), [Song song](screenshots/lab6-parallel.png).

### Chạy Lab 6

```powershell
powershell.exe -ExecutionPolicy Bypass -File Checks/run_lab6_evidence.ps1
```

Hoặc chạy kiểm tra chi tiết & xác minh toàn bộ chứng cứ Lab 6:

```powershell
dotnet build SauceDemo.Tests
dotnet test SauceDemo.Tests --filter "Category=Smoke"
dotnet test SauceDemo.Tests --filter "Category!=Slow"
dotnet test SauceDemo.Tests --filter "FullyQualifiedName=SauceDemo.Tests.Tests.LoginTests.PageTitle|FullyQualifiedName=SauceDemo.Tests.Tests.LabTests.DynamicLoadingWithoutWait" -l "console;verbosity=detailed"
dotnet test SauceDemo.Tests --filter "FullyQualifiedName=SauceDemo.Tests.Tests.LabTests.DynamicLoadingWithoutWait" -l "console;verbosity=detailed"
dotnet test SauceDemo.Tests --logger "trx;LogFileName=ketqua.trx" --results-directory docs/evidence/lab6
trxlog2html -i docs/evidence/lab6/ketqua.trx -o docs/evidence/lab6/ketqua.html
dotnet run --project Checks/Lab5Checks.csproj -- --require-evidence
```

## Lab 5 — Framework, cấu hình và dữ liệu ngoài

Phần này là hướng dẫn cho source hiện tại; hướng dẫn Cốc Cốc và cấu trúc Framework bên dưới thuộc các Lab trước. Nâng cấp trực tiếp project SauceDemo.Tests, không thêm Checkout.

- `Config/`: TestConfig, ConfigReader và appsettings.json. Đọc từ AppContext.BaseDirectory bằng Configuration.Json + Binder; JSON được chép ra output. BaseUrl và LabBaseUrl đều nằm trong JSON.
- `Core/`: DriverFactory trả IWebDriver, ánh xạ enum sang Chrome/Firefox/Edge/CocCoc; báo đúng tên nhập sai. Headless và kích thước cửa sổ áp dụng cho mọi trình duyệt. Tên kiểu Selenium trong factory là ánh xạ cần thiết; không có URL hoặc chuỗi lựa chọn trình duyệt viết cứng trong C#.
- `Core/BaseTest.cs`: đọc config, tạo driver, mở URL; giữ log và chụp ảnh Failed qua `Utilities/ScreenshotHelper.cs`. Quit và Dispose nằm trong finally riêng, kể cả khi chụp ảnh hoặc Quit gặp lỗi; dọn driver nếu mở URL ở SetUp lỗi.
- `Pages/`, `Tests/`: giữ POM và kiểm tra nghiệp vụ cũ. DaHienThi dùng FindElements, chờ tối đa 3 giây rồi trả false khi timeout. Test đăng nhập kiểm tra bool ThanhCong cho cả ca thuận/nghịch và giữ nguyên thông báo lỗi cũ.
- `Data/users.json`: nguồn hiện hành; Username, Password, ThanhCong, GhiChu; thêm Error để bảo toàn kiểm tra thông báo lỗi Lab 1–4. `login.json` là dữ liệu cũ được giữ nguyên. Nguồn static sinh một TestCaseData mỗi dòng với tên DangNhap_<chỉ số>_<Username>, tránh trùng tên khi username rỗng.
- `screenshots/`, `docs/evidence/lab5/`: minh chứng mới, tách biệt hồ sơ Lab 4.

### Chạy Lab 5

```powershell
dotnet build SauceDemo.Tests
dotnet test SauceDemo.Tests --logger 'trx;LogFileName=lab5.trx' --results-directory TestResults/lab5
dotnet run --project Checks/Lab5Checks.csproj
```

Sửa duy nhất Browser trong `SauceDemo.Tests/Config/appsettings.json`: Chrome (mặc định), Firefox, Edge hoặc CocCoc. Không cần sửa C#. CocCocBinary nằm trong JSON; sửa đường dẫn đó nếu Cốc Cốc cài ở vị trí khác. Cốc Cốc dùng ChromeOptions/ChromeDriver vì cùng nền Chromium, có headless và kích thước cửa sổ như Chrome. Driver phải khớp phiên bản Chromium của Cốc Cốc; trên máy này dùng driver 154.0.8037.92 cho Cốc Cốc 154.

Xóa biến TEST_BROWSER nếu muốn lấy lựa chọn từ JSON; biến này được giữ để CI cũ tiếp tục chạy. BROWSER_BINARY (hoặc CHROME_BINARY cũ) chỉ dành cho binary cài ở vị trí riêng; CHROMEDRIVER_DIRECTORY chỉ dành cho driver Chromium khớp phiên bản. Khi chuyển trình duyệt cần xóa các override binary/driver của trình duyệt trước. Selenium Manager tìm/tải driver mặc định.

Thêm một object đúng schema vào users.json rồi chạy lại: số ca tăng mà không sửa test. Không chạy Explicit DynamicLoadingWithoutWait trong bộ thường lệ. Script check_lab4.py dùng hash/source baseline Lab 4, nên không dùng để đánh giá source Lab 5 đã đổi cấu trúc và dữ liệu; hồ sơ Lab 4 được giữ nguyên để kiểm tra trên commit cũ.

Kiểm chứng local: build 0 warning/0 error; Cốc Cốc 20/20 PASS với 11 bộ dữ liệu; Chrome và Firefox 19/19 PASS với 10 bộ trước đó. Trên source cuối, chạy riêng DynamicLoading bằng Chrome rồi Firefox cũng PASS, chỉ đổi Browser trong JSON. [Hồ sơ thực thi và giới hạn](docs/evidence/lab5/README.md), [ảnh Chrome](screenshots/lab5-chrome.png), [ảnh Firefox](screenshots/lab5-firefox.png), [ảnh Cốc Cốc](screenshots/lab5-coccoc.png), [dữ liệu trước](screenshots/lab5-data-before.png), [sau](screenshots/lab5-data-after.png).

### Phần E — prompt, toàn bộ kết quả AI và thẩm định

Prompt dùng trong phiên Codex này: “Sinh 10 bộ dữ liệu kiểm thử đăng nhập SauceDemo theo schema Username (string), Password (string), ThanhCong (bool), GhiChu (string). Có ca thuận, tài khoản khóa, sai mật khẩu, ô trống, khoảng trắng và ký tự đặc biệt. Đây là đề xuất cần thẩm định, chưa phải kỳ vọng đã xác minh.”

Toàn bộ 10 đề xuất do Codex sinh trong phiên này (không phải lời khai về một dịch vụ AI khác):

```json
[
  {"Username":"standard_user","Password":"secret_sauce","ThanhCong":true,"GhiChu":"Đăng nhập hợp lệ"},
  {"Username":"performance_glitch_user","Password":"secret_sauce","ThanhCong":true,"GhiChu":"Tài khoản phản hồi chậm"},
  {"Username":"locked_out_user","Password":"secret_sauce","ThanhCong":false,"GhiChu":"Tài khoản bị khóa"},
  {"Username":"standard_user","Password":"wrong","ThanhCong":false,"GhiChu":"Mật khẩu sai"},
  {"Username":"","Password":"secret_sauce","ThanhCong":false,"GhiChu":"Username rỗng"},
  {"Username":"standard_user","Password":"","ThanhCong":false,"GhiChu":"Password rỗng"},
  {"Username":"","Password":"","ThanhCong":false,"GhiChu":"Cả hai ô rỗng"},
  {"Username":"' OR 1=1 --","Password":"secret_sauce","ThanhCong":false,"GhiChu":"Ký tự đặc biệt"},
  {"Username":" standard_user ","Password":"secret_sauce","ThanhCong":true,"GhiChu":"Ứng dụng tự trim khoảng trắng"},
  {"Username":"admin","Password":"admin","ThanhCong":true,"GhiChu":"Đăng nhập quản trị và mở dashboard admin"}
]
```

Hai bộ không dùng được nguyên trạng:

1. Bộ 9 kỳ vọng true dựa trên giả định ứng dụng tự trim username. Trang thực tế từ chối username có khoảng trắng; dữ liệu được sửa thành false và kiểm tra đúng thông báo mismatch. Không sửa ứng dụng hoặc nới assert theo kỳ vọng AI.
2. Bộ 10 tự bịa tài khoản admin/admin và chức năng dashboard quản trị. Không có căn cứ xác nhận tài khoản/quyền này trên SauceDemo, nên loại khỏi tập chạy; không coi đề xuất AI là tài khoản thật. Có thể thiết kế thành ca âm riêng nếu cần, với kỳ vọng đã xác minh.

Các đề xuất được đối chiếu với dữ liệu Lab trước và kết quả browser. users.json không chép mù toàn bộ đầu ra AI: giữ ca unknown-user cũ và thông báo lỗi gốc, sửa kỳ vọng khoảng trắng, bổ sung ca biên độc lập. Không có mật khẩu thật hoặc credential riêng trong cấu hình.

[![Tests](https://github.com/phuongvhn2009/KiemThu/actions/workflows/tests.yml/badge.svg)](https://github.com/phuongvhn2009/KiemThu/actions/workflows/tests.yml)

Framework kiểm thử đăng nhập và giỏ hàng SauceDemo cho ASM kiểm thử nâng cao cùng AI. C#/.NET 10, NUnit 4, Selenium 4, POM, explicit wait, dữ liệu JSON. Local dùng **Cốc Cốc**; CI dùng Chrome headless.

## Cài đặt và chạy (PowerShell)

Cần .NET SDK 10, Cốc Cốc và mạng tới NuGet/SauceDemo. ChromeDriver phải cùng major Chromium với Cốc Cốc (máy kiểm chứng: 154.0.8037.106, driver 154.0.8037.92).

```powershell
git clone https://github.com/phuongvhn2009/KiemThu.git
cd KiemThu
dotnet restore SauceDemo.Tests
$env:TEST_ARTIFACTS = "$PWD/TestResults/artifacts"
# Nếu Manager không nhận diện Cốc Cốc: trỏ tới thư mục driver tương thích đã tải.
$env:CHROMEDRIVER_DIRECTORY = "$env:USERPROFILE/.cache/selenium/chromedriver/win64/154.0.8037.92"
dotnet test SauceDemo.Tests --logger "trx;LogFileName=tests.trx" --results-directory TestResults
```

Chỉ đặt CHROMEDRIVER_DIRECTORY khi thư mục tồn tại; đổi phiên bản theo máy. Bỏ biến để Selenium Manager tự tải. Cốc Cốc ở vị trí khác: đặt CHROME_BINARY tới browser.exe. Mặc định Windows tìm Program Files/CocCoc/Browser/Application/browser.exe. CI/Linux: TEST_BROWSER=chrome, không đặt đường dẫn Windows.

## Cấu hình và cấu trúc (Lịch sử Lab 1–4)

*(Lưu ý: Phần bên dưới lưu lại tài liệu lịch sử của Lab 1–4; cấu trúc hiện hành của Lab 5 gồm `Config/`, `Core/`, `Pages/`, `Data/`, `Utilities/`, `Tests/` đã mô tả ở đầu file).*

appsettings.json chứa BaseUrl HTTPS, Browser (coccoc/chrome), TimeoutSeconds dương, Headless. TEST_BROWSER ghi đè browser; CHROME_BINARY ghi đè executable. Selenium tạo profile tạm riêng.

```text
SauceDemo.Tests/
  Framework/   Settings, BaseTest (driver/log/screenshot)
  Pages/       BasePage, LoginPage, InventoryPage, CartPage
  Tests/       LoginTests, CartTests
  Data/        login.json (8 ca)
.github/workflows/tests.yml
docs/          báo cáo, thẩm định AI và TRX minh chứng
screenshots/   ảnh lỗi thực tế
```

10 test: tiêu đề, 8 ca login thường/biên/lỗi, thêm Backpack vào giỏ và kiểm tra tên/số lượng. Hai fixture song song, driver riêng từng test. Log theo test; ảnh lỗi khi driver đã mở được. TRX tạo bằng --logger trx. CI upload artifact test-evidence kể cả khi lỗi. Không đặt TEST_ARTIFACTS thì log/ảnh ở artifacts trong NUnit WorkDirectory (thường thư mục build).

## POM và minh chứng

Đổi id nút login chỉ cần sửa field submit trong LoginPage; 8 ca dữ liệu không đổi. CartPage chờ locator thuộc cart-list tránh đọc Inventory khi chuyển trang. BasePage chờ hiển thị/khả dụng; test performance_glitch_user kiểm tra chuyển trang chậm.

![Ảnh lỗi thật trước khi sửa locator](screenshots/initial-failure.png)

[PR #1](https://github.com/phuongvhn2009/KiemThu/pull/1) · [Thẩm định AI](docs/ai-review.md) · [Báo cáo](docs/report.md) · [TRX](docs/evidence/)

GitHub Actions bị khóa bởi billing tài khoản: job chưa chạy, badge đỏ không chứng minh test sai. Sau khi gỡ khóa cần chạy lại và kiểm tra xanh. Pin repo/bio/avatar GitHub cần chủ tài khoản lựa chọn; chưa ghi nhận đã hoàn thiện.

Dữ liệu login là tài khoản demo công khai SauceDemo. Không đưa token, credential hay dữ liệu cá nhân lên repo. Báo cáo ghi rõ phần đã kiểm chứng và phần còn thiếu.

## Lab 1–3 tích hợp

Repo duy nhất: https://github.com/phuongvhn2009/KiemThu. Xem [bảng đối chiếu](docs/lab-requirements.md). Một project SauceDemo.Tests, giữ POM, JSON login và explicit wait.

- Lab 1: PageTitle in và kiểm tra Swag Labs; BaseTest đóng và Dispose driver.
- Lab 2: Authenticate kiểm tra URL inventory.html hoặc đúng thông báo lỗi; Products lấy 6 sản phẩm bằng item.FindElement, in tên–giá, kiểm tra tên không rỗng, tiền tố $ và Backpack.
- Lab 3: PricesAscending dùng SelectElement, decimal và Assert thứ tự; DynamicLoading, JavaScriptAlert, Iframe dùng the-internet.herokuapp.com. DynamicLoadingWithoutWait là ca Explicit cố ý rớt, chạy riêng.

```powershell
dotnet test SauceDemo.Tests --logger 'trx;LogFileName=labs.trx' --results-directory TestResults/labs
dotnet test SauceDemo.Tests --filter 'Name=DynamicLoadingWithoutWait' --logger 'trx;LogFileName=intentional.trx' --results-directory TestResults/intentional
```

Lệnh thứ hai dự kiến trả mã lỗi và tạo ảnh đính kèm: đó là minh chứng chưa chờ #finish. JavaScript tạo phần tử sau khi trang đã tải xong; Explicit Wait chờ sự xuất hiện nên ca DynamicLoading PASS. Không dùng performance_glitch_user để thay minh chứng này.

### Locator Lab 2

| Phần tử | Chọn | Phương án khác | Lý do |
|---|---|---|---|
| Username | #user-name | [data-test='username'] | id định danh ô nhập |
| Password | #password | [data-test='password'] | id ổn định |
| Login | #login-button | [data-test='login-button'] | id định danh nút |
| Tên Backpack | #item_4_title_link | [data-test='inventory-item'] [data-test='inventory-item-name'] | id cho một sản phẩm; selector thứ hai dùng trong từng item |
| Giá Backpack | [data-test='inventory-item']:first-child [data-test='inventory-item-price'] | .inventory_item:first-child .inventory_item_price | data-test ít phụ thuộc CSS; first-child chỉ dùng kiểm chứng thứ tự mặc định |

Ảnh Ctrl+F và prompt outerHTML/nhận xét AI lịch sử nằm trong báo cáo Lab 2 ở DOCUMENT; chưa chụp lại DevTools cho bản tích hợp. Trong code danh sách, selector tên và giá được giới hạn trong từng item, không yêu cầu selector danh sách khớp duy nhất trên cả trang.

### Ghi nhận AI và bằng chứng mới

AI phát hiện phần còn thiếu bằng đối chiếu PDF với code. Lần chạy đầu: 15 PASS, 2 FAIL. Alert lỗi vì gửi chữ vào hộp thoại không có ô nhập; sửa chỉ SendKeys với Prompt. Iframe trả chuỗi rỗng sau Clear/SendKeys; thay bằng click, Ctrl+A, Delete và chờ nội dung thực tế. Chạy lại verified: 17/17 PASS trong 36 giây. Đây chỉ là một lần chạy; không tuyên bố đã lặp 15–20 lần. Log/TRX ở docs/evidence/labs. Prompt tác vụ tích hợp là yêu cầu người dùng ngày 12/11/2026; các prompt AI gốc của từng lab được giữ trong Word.

NuGet có cảnh báo NU1900 do SSL khi kiểm tra dữ liệu lỗ hổng; không tắt audit. Các trạng thái CI trong phần ASM phía trên là lịch sử, chưa xác minh lại trên GitHub. Không commit/push trong lần tích hợp này.

## Lab 4 — Refactor POM

### Kết quả mới ngày 14/11/2026

**Đã bàn giao trên GitHub:** [nhánh refactor/lab4-pom](https://github.com/phuongvhn2009/KiemThu/tree/refactor/lab4-pom), commit code [dcd7506](https://github.com/phuongvhn2009/KiemThu/commit/dcd75060bdbd47eab8161ec2f7b7d578b3ff2cef). [CI của đúng commit](https://github.com/phuongvhn2009/KiemThu/actions/runs/43617220591) được kích hoạt nhưng GitHub không khởi chạy job vì tài khoản bị khóa do billing; chưa có kết quả test CI. [Thông báo API gốc](docs/evidence/lab4/session-20261114/ci-code-commit-annotations.json). Chủ tài khoản cần xử lý billing rồi chạy lại workflow; không sửa test hoặc workflow để che trạng thái này.

Cập nhật lúc hoàn tất: chạy lại restore ngoài sandbox đã tải thành công dữ liệu lỗ hổng NuGet, không tắt audit hay đổi dependency; build cuối **0 warning, 0 error**. NU1900 trong các log trước là kết quả thật của giới hạn SSL trong sandbox. Xem [log restore](docs/evidence/lab4/session-20261114/restore-recheck-unrestricted.log) và [build cuối](docs/evidence/lab4/session-20261114/final-build.log). Người dùng đã yêu cầu tiếp tục commit/push và kiểm tra CI; các câu “chưa commit/push” bên dưới mô tả trạng thái ở thời điểm ghi nhận trước đó.

Nhánh `refactor/lab4-pom`, nền `7ae637d78f16a68c8c67d2de539c6e46e72d569e` **cộng thay đổi chưa commit có sẵn**. Đã đọc lại đề A–E và giữ công việc người dùng. Xem [hồ sơ phiên này](docs/evidence/lab4/session-20261114/README.md) để đối chiếu commit, trạng thái Git, source trước sửa, hash, danh sách test, log, TRX và diff. Không dùng số liệu cũ bên dưới làm baseline mới.

Build trước/sau thành công; bộ thường lệ **17/17 PASS trước và sau**, cùng tên và outcome; Explicit NotExecuted. Ca `DynamicLoadingWithoutWait` chạy riêng FAIL đúng `NoSuchElementException` tại `#finish`. `python Checks/check_lab4.py` PASS, mặc định đọc minh chứng phiên 14/11. Build còn NU1900 do SSL khi audit NuGet.

Đã xác minh A–D: BasePage abstract quản lý Driver/Wait/Tim; locator private readonly; đủ API đăng nhập, inventory và chaining giá; Tests không có By, Pages không có Assert hoặc public API trả IWebElement. Sửa thêm OpenCart trả CartPage và cập nhật caller; chuyển iframe bằng phần tử tìm từ locator chung. Giữ toàn bộ assert cũ, dữ liệu JSON và chức năng Lab 1–3/Assignment.

### Phần E — prompt, kết quả và thẩm định AI

Prompt thực tế là yêu cầu hoàn thiện Lab 4 của người dùng trong phiên này, với quy ước từ đề PDF đã đọc. Các trích đoạn nguyên văn liên quan:

> “Hoàn thiện các phần A–E của Lab 4 trên project SauceDemo.Tests hiện có.”
> “BasePage phải abstract, quản lý driver và WebDriverWait dùng chung.”
> “Có hàm protected Tim(By) tìm phần tử kèm explicit wait.”
> “Giữ nguyên chức năng, dữ liệu và các kiểm tra của Lab 1–3 cùng Assignment.”

Quy ước đối chiếu thêm từ đề: locator private readonly By; không Assert trong Page; không By trong Tests; hành vi chuyển trang trả Page mới; không trả IWebElement qua API public. Đây là bản ghi prompt và tiêu chí đã dùng, không dựng thêm cuộc trò chuyện AI khác.

Kết quả Codex trả về trong phiên này là [diff code thực tế](docs/evidence/lab4/session-20261114/session-code.diff): refactor CartTests trong UiTests.cs để nhận `var cart = inventory.OpenCart();`, InventoryPage.OpenCart trả CartPage, LabPage chuyển frame bằng `Tim(frame)`. Assert tên Backpack/số lượng và iframe giữ nguyên. Những thay đổi POM lớn khác đã có trước phiên và được kiểm tra lại, không nhận là đầu ra mới.

Chỗ AI chưa đúng quy ước trong bản có sẵn: `OpenCart()` trả void, test tự `new CartPage(...)`; việc chuyển sang Page mới chưa được mô hình hóa tại hành vi chuyển trang theo hướng dẫn đề. Đã sửa Page và caller. Iframe còn lặp ID chuỗi dù có locator private readonly, đã dùng trực tiếp phần tử từ Tim(frame). Nhận xét cũ “Assert còn trong LabPage” và “By còn trong test iframe” **không còn đúng ở source đầu phiên**; không nhận công sửa lỗi đã hết.

Kiểm chứng: đọc caller, rà quy ước Pages/Tests và assert, so sánh mapping tên/outcome của hai TRX; hash xác nhận Framework/Data/config/dependency không đổi; chạy riêng Explicit để giữ minh chứng lỗi đồng bộ. Không nới assert để ép xanh.

![Lab 4 trước sửa tiếp, phiên 14/11](screenshots/lab4-20261114-before.png)
![Lab 4 sau sửa, phiên 14/11](screenshots/lab4-20261114-after.png)

Hai ảnh trên là browser chụp trang hiển thị log nguyên văn và dữ liệu TRX phiên này, có nguồn/hash; không giả giao diện terminal/Test Explorer. Ảnh browser từ các test có attachment cũng nằm trong hồ sơ phiên. Không tạo Word, sửa form tài liệu hay triển khai Checkout. Đã commit/push toàn bộ phần Lab 4 và kiểm tra CI theo yêu cầu tiếp theo; file cấu hình IDE cá nhân vẫn giữ local.

### Ghi nhận cũ có sẵn khi bắt đầu phiên

[Phân tích, đối chiếu từng tiêu chí, API, thẩm định AI và bằng chứng Lab 4](docs/lab4.md).

- BasePage abstract giữ Driver và WebDriverWait; Tim(By) tìm phần tử kèm explicit wait. Page con kế thừa, locator private readonly.
- Login.DangNhap trả InventoryPage; LayThongBaoLoi giữ luồng login sai. InventoryPage có DaHienThi, SapXep(value) trả this và LayDanhSachGia trả IReadOnlyList<decimal>.
- Test sắp xếp dùng `Login.DangNhap(...).SapXep("lohi").LayDanhSachGia()`, vẫn kiểm tra đủ 6 giá và Is.Ordered.Ascending.
- Tests không còn chữ By; Pages không có Assert và không trả IWebElement qua API public. Nội dung alert được Assert ở test trước khi đóng; iframe được kiểm tra qua Page.
- Trước: 17/17 PASS (49 giây). Sau: cùng 17/17 PASS (53 giây). Explicit không chờ vẫn rớt riêng đúng NoSuchElementException. Log/TRX ở [docs/evidence/lab4](docs/evidence/lab4/); kiểm tra tự động: `python Checks/check_lab4.py`.
- Thẩm định AI: code trước có Assert trong LabPage và By trong test iframe, đã chuyển về đúng trách nhiệm; bản nháp có nullable warning ở alert đã sửa. Prompt và phân tích chi tiết nằm trong tài liệu Lab 4.

Ảnh dưới là ảnh chụp trang hiển thị log thực thi nguyên văn, không phải Test Explorer; đối chiếu được với TRX kèm theo.

![Lab 4 trước refactor](screenshots/lab4-before.png)
![Lab 4 sau refactor](screenshots/lab4-after.png)
