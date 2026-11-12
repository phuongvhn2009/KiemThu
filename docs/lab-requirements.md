# Đối chiếu Lab 1–3

Ngày tích hợp: 12/11/2026. Một project NUnit: SauceDemo.Tests. Repo: https://github.com/phuongvhn2009/KiemThu.

| Lab / yêu cầu | Trước tích hợp | Bổ sung / bằng chứng cần kiểm tra |
|---|---|---|
| 1A SDK, IDE, NUnit, Selenium | Có project và ảnh trong Word | Giữ ảnh lịch sử; log môi trường mới trong docs/evidence/labs |
| 1B mở trang, in title, Assert, Quit | Có Assert title, chưa in title | UiTests.PageTitle in Swag Labs; BaseTest Quit và Dispose |
| 1C public repo, gitignore, ≥2 commit | Có gitignore và lịch sử | Giữ lịch sử; không tạo commit theo yêu cầu |
| 1D AI prompt và nhận xét | Có trong Word | Giữ nội dung lịch sử, không coi là kết quả chạy mới |
| 2A 5 phần tử, ≥2 locator, Ctrl+F | Có bảng và ảnh trong Word | Bảng trong README; ảnh cũ chưa được chụp lại |
| 2B login thuận/nghịch, URL và lỗi | Có dữ liệu JSON, Assert Products và lỗi | Thêm Assert URL inventory.html |
| 2C 6 sản phẩm, item.FindElement, tên–giá, Assert | Thiếu | InventoryPage.Products và LabTests.Products |
| 2D AI outerHTML, locator và nhận xét | Có trong Word | Giữ prompt; README dẫn nguồn |
| 2E ≥3 Conventional Commits cùng repo | Lịch sử có sẵn | Không commit/push mới; không xác nhận hash cũ của repo khác |
| 3A dynamic_loading/2 rớt và pass | Thiếu | LabPage, DynamicLoading, DynamicLoadingWithoutWait Explicit |
| 3B SelectElement, 6 giá số tăng dần | Thiếu | PricesAscending và InventoryPage.SortPrice |
| 3C 3 alert, Assert text/result, iframe và DefaultContent | Thiếu | JavaScriptAlert, Iframe; đúng website the-internet |
| 3D screenshot chỉ khi Failed, attachment | Có | Giữ cơ chế; đổi tên ảnh theo tên test |
| 3E AI chẩn đoán, prompt, đánh giá | Báo cáo cũ có số lần chưa xác minh | Xóa kết luận lặp PASS chưa có log; ghi kết quả thực tế riêng |

Không dùng Thread.Sleep hoặc implicit wait. Ca cố ý rớt chạy riêng để không làm đỏ bộ thường lệ. PDF gốc không sửa. Ảnh lịch sử trong Word không chứng minh bản tích hợp mới.
