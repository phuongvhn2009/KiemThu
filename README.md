
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
# Nếu Manager không nhận diện Cốc Cốc: trỏ tới thư mục driver tương thích đã tải.
$env:CHROMEDRIVER_DIRECTORY = "$env:USERPROFILE/.cache/selenium/chromedriver/win64/154.0.8037.92"
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
