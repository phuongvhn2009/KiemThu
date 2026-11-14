# Lab 4 — phiên kiểm chứng ngày 14/11/2026

Nhánh: `refactor/lab4-pom`. Remote: `https://github.com/phuongvhn2009/KiemThu.git`.
Commit nền: `7ae637d78f16a68c8c67d2de539c6e46e72d569e`.

## Nhận diện baseline

Khi bắt đầu, main đã có thay đổi Lab 4 chưa commit. Baseline **không phải HEAD sạch**: đây là source đang làm của người dùng, đã có phần lớn POM. Không ghi đè hay đảo ngược những thay đổi này để tạo ảnh “trước” khác phiên bản.

- `initial-status.txt`: trạng thái Git lúc bắt đầu, sau khi tạo nhánh.
- `initial-working-tree.patch`: thay đổi tracked so với commit nền trước khi sửa.
- `before-source.zip`: bản chụp source/config/data thực sự chạy trước khi sửa trong phiên này; bỏ bin/obj và cấu hình IDE. Đây là bản lưu minh chứng, không phải project test thứ hai trong solution.
- `before-source-sha256.json`: hash từng file của bản chụp.
- `session-code.diff`: chỉ thay đổi code phát sinh trong phiên này, so với bản chụp; chuẩn hóa xuống dòng để dễ đọc.
- `after-source-sha256.json`: hash source cuối.

Không tìm thấy AGENTS.md trong repo hoặc D:/; AGENTS.md của Codex được kiểm tra là file rỗng. Đã đọc README, các Page/Test/Framework, cấu hình, dữ liệu JSON và PDF Lab 4. Bản PDF trong Downloads trùng SHA-256 với DOCUMENT/02_11_2026___8ce62c9e-8bdd-453d-bdbf-59b20e4a9ecf.pdf: `EC3F84EE6CC84B1832D49C4B12E271E0D915C48C261BA32F54DB660169403689`.

## Chạy thật

Cập nhật ở bước bàn giao: `restore-recheck.log` tái hiện NU1900 trong sandbox; `restore-recheck-unrestricted.log` chạy cùng restore ngoài sandbox đã GET thành công dữ liệu audit, 0 warning/0 error. `final-build.log` xác nhận build cuối 0 warning/0 error. Không sửa config, dependency, trust store hay tắt audit để che cảnh báo. Các log trước/sau ban đầu dưới đây được giữ nguyên.

Cốc Cốc headless, ChromeDriver 154.0.8037.92 trong cache, .NET SDK 10.0.403; appsettings và JSON giữ nguyên. Không thay assert để ép PASS.

| Lần chạy | Kết quả | Tệp |
|---|---|---|
| Build trước | PASS, 0 error, NU1900 | before-build.log |
| Test trước | 17 PASS; Explicit NotExecuted | before.log, before.trx, before-test-list.log |
| Build sau | PASS, 0 error, NU1900 | after-build.log |
| Test sau | 17 PASS; Explicit NotExecuted | after.log, after.trx, after-test-list.log |
| Explicit riêng sau | FAIL dự kiến, NoSuchElementException tại #finish | intentional-after.log, intentional-after.trx |
| Quy ước + so sánh | Kết quả trong checks.log | Checks/check_lab4.py |

NU1900 báo SSL khi lấy dữ liệu lỗ hổng NuGet; không tắt audit. Mỗi bộ thường lệ chỉ chạy một lần; không kết luận độ ổn định dài hạn hay hiệu năng từ thời gian chạy. TRX có 18 entry gồm 17 Passed và 1 NotExecuted; ca Explicit chỉ được chọn trong lệnh riêng.

`before-artifacts/`, `after-artifacts/`, `intentional-artifacts/` giữ log START/END thật của từng test. TRX đính kèm ảnh browser DynamicLoading PASS và ảnh lỗi cố ý; đường dẫn gốc có thể chứa đường dẫn tuyệt đối của máy chạy, dùng bản attachment đã sao chép trong thư mục kết quả khi xem trên máy khác.

Ảnh tổng hợp trong screenshots/lab4-20261114-before.png và lab4-20261114-after.png là **browser chụp trang hiển thị log nguyên văn và kết quả từ TRX phiên này**; không phải ảnh terminal/Test Explorer. Trên ảnh có tên source, SHA-256 của TRX và nhãn baseline. Không dựng giao diện IDE hoặc sửa số liệu. Ảnh lab4-before.png/lab4-after.png và TRX ở thư mục cha là tư liệu cũ, không dùng thay baseline phiên này.

## Lệnh đã chạy

```powershell
$env:CHROMEDRIVER_DIRECTORY='C:/Users/ADMIN-PC/.cache/selenium/chromedriver/win64/154.0.8037.92'
dotnet build SauceDemo.Tests --no-restore
dotnet test SauceDemo.Tests --no-build --list-tests
# TEST_ARTIFACTS được đặt tới thư mục before-artifacts hoặc after-artifacts tương ứng.
dotnet test SauceDemo.Tests --no-build --logger 'trx;LogFileName=before.trx' --results-directory docs/evidence/lab4/session-20261114
dotnet test SauceDemo.Tests --no-build --logger 'trx;LogFileName=after.trx' --results-directory docs/evidence/lab4/session-20261114
dotnet test SauceDemo.Tests --no-build --filter 'Name=DynamicLoadingWithoutWait' --logger 'trx;LogFileName=intentional-after.trx' --results-directory docs/evidence/lab4/session-20261114
python Checks/check_lab4.py
```

Trên máy này dùng Python trong runtime Codex vì lệnh python hệ thống là Windows Store alias. Log shell có encoding UTF-16LE; script kiểm tra đọc source UTF-8 và TRX XML.
