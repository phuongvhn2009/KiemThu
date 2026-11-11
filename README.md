# SauceDemo - NUnit + Selenium

[![Tests](https://github.com/phuongvhn2009/KiemThu/actions/workflows/tests.yml/badge.svg)](https://github.com/phuongvhn2009/KiemThu/actions/workflows/tests.yml)

Framework kiểm thử đăng nhập và giỏ hàng SauceDemo cho ASM kiểm thử nâng cao cùng AI. C#/.NET 10, NUnit 4, Selenium 4, POM, explicit wait, dữ liệu JSON. Local dùng **Cốc Cốc**; CI dùng Chrome headless.

## Cài đặt và chạy (PowerShell)

Cần .NET SDK 10, Cốc Cốc và mạng tới NuGet/SauceDemo. ChromeDriver phải cùng major Chromium với Cốc Cốc (máy kiểm chứng: 154.0.8037.106, driver 154.0.8037.92).

```powershell
git clone https://github.com/phuongvhn2009/KiemThu.git
cd KiemThu
dotnet restore SauceDemo.Tests
$env:TEST_ARTIFACTS = "$PWD/TestResults/artifacts"
# Chạy local bằng Cốc Cốc: đổi thành "coccoc". CI dùng "chrome".
$env:TEST_BROWSER = "chrome"
dotnet test SauceDemo.Tests --logger "trx;LogFileName=tests.trx" --results-directory TestResults
```

Chỉ đặt CHROMEDRIVER_DIRECTORY khi thư mục tồn tại; đổi phiên bản theo máy. Bỏ biến để Selenium Manager tự tải. Cốc Cốc ở vị trí khác: đặt CHROME_BINARY tới browser.exe. Mặc định Windows tìm Program Files/CocCoc/Browser/Application/browser.exe. CI/Linux: TEST_BROWSER=chrome, không đặt đường dẫn Windows.

## Cấu hình và cấu trúc

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

10 test: tiêu đề, 8 ca login thường/biên/lỗi, thêm Backpack vào giỏ và kiểm tra tên/số lượng. Hai fixture song song, driver riêng từng test. Log theo test; ảnh lỗi khi driver đã mở được. TRX tạo bằng --logger trx. CI upload artifact test-evidence kể cả khi lỗi. Không đặt TEST_ARTIFACTS thì log/ảnh ở artifacts trong NUnit WorkDirectory (thường là thư mục build).

## POM và minh chứng

Đổi id nút login chỉ cần sửa field submit trong LoginPage; 8 ca dữ liệu không đổi. CartPage chờ locator thuộc cart-list để tránh đọc nhầm trang Inventory khi chuyển trang. BasePage chờ phần tử hiển thị và khả dụng; test performance_glitch_user kiểm tra trường hợp trang chuyển chậm.

![Trang giỏ hàng sau khi thêm Sauce Labs Backpack, ảnh chụp khi lỗi StaleElement trong lần chạy đầu](screenshots/initial-failure.png)

[PR #1](https://github.com/phuongvhn2009/KiemThu/pull/1) · [Thẩm định AI](docs/ai-review.md) · [Báo cáo](docs/report.md) · [TRX](docs/evidence/)

Dữ liệu login là tài khoản demo công khai của SauceDemo. Không đưa token, credential hay dữ liệu cá nhân lên repo.
