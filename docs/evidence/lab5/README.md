# Lab 5 — phiên 16/11/2026

Nâng cấp trực tiếp SauceDemo.Tests trên nhánh feat/lab5-framework. Không chỉnh minh chứng Lab 4, Word/PDF hay Checkout. File IDE cá nhân vẫn ở local, ngoài phạm vi.

## Kết quả thực thi

| Lượt | Dữ liệu | Kết quả | File |
|---|---|---|---|
| Chrome 157, trước khi bổ sung Cốc Cốc | 10 bộ | 19 PASS | chrome-before.trx |
| Firefox 160, ngoài sandbox | 10 bộ | 19 PASS | firefox-before.trx |
| Cốc Cốc 154, source đã hỗ trợ Cốc Cốc | 11 bộ | 20 PASS | coccoc-final.trx |
| Build cuối | Source Lab 5 | 0 warning, 0 error | build.log |
| Kiểm tra độc lập | Config/driver/POM/data | PASS | checks.log |
| DynamicLoading trên source cuối — Chrome | 11 bộ được discovery, lọc chạy 1 ca | 1 PASS | chrome-final-smoke.trx |
| DynamicLoading trên source cuối — Firefox | 11 bộ được discovery, lọc chạy 1 ca | 1 PASS | firefox-final-smoke.trx |

DynamicLoadingWithoutWait là Explicit, không chạy trong các bộ thường lệ. Không nới assertion hoặc bỏ test cũ. 8 dòng login cũ chuyển sang users.json giữ nguyên Error; thêm kiểm tra trạng thái inventory cho cả ca âm. Bổ sung khoảng trắng thừa, ký tự đặc biệt và mật khẩu dài 256 ký tự.

Cốc Cốc được chọn bằng Browser=CocCoc trong JSON; CocCocBinary trỏ tới bản cài thực tế, driver 154.0.8037.92. Chrome dùng bản trong cache Selenium, Firefox/geckodriver do Selenium Manager tải vào cache. Hai lượt DynamicLoading cuối chỉ đổi Browser trong JSON, không override binary/driver và không sửa source giữa hai lượt; sau đó khôi phục CocCoc. Không cài thay trình duyệt mặc định của hệ điều hành.

Các lượt đầu phát hiện lỗi chuyển JSON bằng PowerShell (mảng bị lồng); đã sửa và chạy lại. Một lượt Firefox trong sandbox bị kẹt lúc tạo browser đã dừng, không coi là PASS; lượt ngoài sandbox trong bảng mới là kết quả đầy đủ. Không dùng các lượt lỗi này làm minh chứng thành công. Các file nguyên thủy của những lượt đó vẫn ở TestResults (gitignored).

## Minh chứng thêm dữ liệu không sửa C#

- users-before.json: 10 bộ. tests-before.log: 10 tên DangNhap, 19 ca thường lệ cộng một Explicit được discovery.
- users-after.json: 11 bộ. tests-after.log: 11 tên DangNhap, 20 ca thường lệ cộng một Explicit được discovery.
- source-before-data-add.json và source-after-data-add.json trùng toàn bộ hash C# trong SauceDemo.Tests.
- Khi khôi phục JSON có timestamp cũ, PreserveNewest có thể để lại dữ liệu output mới hơn. Đã chuyển CopyToOutputDirectory sang Always và thực hiện lại discovery trước/sau. Hai log trong hồ sơ là kết quả discovery thật sau khi sửa cơ chế copy.
- screenshots/lab5-data-before.png và lab5-data-after.png là ảnh browser hiển thị log discovery nguyên văn, có ghi rõ không phải Test Explorer. Không chỉnh sửa ảnh để thay số lượng test.
- lab5-chrome.png và lab5-firefox.png là ảnh trang thật từ attachment DynamicLoading của hai lượt smoke cuối; lab5-coccoc.png từ lượt Cốc Cốc 20 PASS.

Prompt, đủ 10 đề xuất dữ liệu AI và hai đề xuất bị bác nằm trong README gốc của repo. Không đưa credential thật vào cấu hình. Bản Lab 5 hiện ở local, chưa publish hoặc xác minh CI cho thay đổi này.
