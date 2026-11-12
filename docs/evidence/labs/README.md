# Bằng chứng tích hợp ngày 12/11/2026

- verified.trx / verified.log: 17 PASS, 0 FAIL, 36 giây.
- final.trx / final.log: 17 PASS, 0 FAIL, 40 giây sau sửa tên ảnh theo test.
- dynamic-pass.trx / dynamic-pass.log: 1 PASS, 10 giây; tạo ảnh screenshots/lab3-wait-pass.png.
- intentional.trx / intentional.log: 1 FAIL có chủ đích, 7 giây, NoSuchElementException tại #finish; ảnh screenshots/lab3-no-wait-failure.png.
- build.log: build thành công, 0 error; NU1900 do SSL khi audit NuGet. Không tắt audit.
- dotnet-info.txt, git-history.txt: môi trường và lịch sử local; không phải xác nhận trạng thái GitHub hiện tại.
- document-check.txt: kiểm tra ZIP/XML và hyperlink của 3 Word, xuất PDF bằng Microsoft Word ẩn; bản xem trước lưu trong .artifacts (không đưa vào Git).

TRX giữ nguyên output NUnit, có đường dẫn attachment tại thư mục chạy local. Bản sao ảnh bền vững đặt ở screenshots/. Test Explicit không nằm trong 17 test thường lệ. Không tuyên bố đã chạy 15–20 lần. Các ảnh Word cũ được ghi rõ là tư liệu lịch sử.

File đã đổi URL bài làm: DOCUMENT/SOF3091_Lab1_TB01973/README.md; ba file Link github.txt; SOF3091_Lab1.docx, SOF3091_Lab2.docx, Lab3.docx (cả XML/hyperlink). README gốc bổ sung hướng dẫn chung; badge CI sẵn dùng KiemThu nên giữ nguyên. Đề PDF không sửa.
