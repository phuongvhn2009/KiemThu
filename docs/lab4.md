# Lab 4 — OOP và Page Object Model

## Hoàn thiện và kiểm chứng ngày 14/11/2026

**Trạng thái bàn giao:** đã push commit code [dcd7506](https://github.com/phuongvhn2009/KiemThu/commit/dcd75060bdbd47eab8161ec2f7b7d578b3ff2cef) lên [refactor/lab4-pom](https://github.com/phuongvhn2009/KiemThu/tree/refactor/lab4-pom). [Run CI 43617220591](https://github.com/phuongvhn2009/KiemThu/actions/runs/43617220591) thất bại trước khi job chạy: GitHub xác nhận tài khoản bị khóa vì billing. Đã lưu JSON runs/jobs/checks/annotations trong hồ sơ phiên; chưa có test nào được CI thực thi. Build local cuối 0 warning/0 error; kiểm thử 17/17 PASS vẫn là kết quả local, không thay thế kết quả CI.

Cập nhật hoàn tất theo yêu cầu tiếp theo của người dùng: tiếp tục commit/push nhánh và xác minh CI. Restore ngoài sandbox truy cập được endpoint audit NuGet; build cuối 0 warning, 0 error, không đổi dependency hoặc tắt audit. Log NU1900 ban đầu được giữ nguyên, không thay bằng kết quả mới. Trạng thái chưa publish trong phần ghi nhận trước đó là lịch sử; xem cập nhật bàn giao trong hồ sơ phiên.

Kết quả hiện hành nằm trong [hồ sơ phiên mới](evidence/lab4/session-20261114/README.md). Các số liệu 49/53 giây và ảnh không có ngày ở phần hồ sơ cũ bên dưới là ghi nhận có sẵn khi bắt đầu, không dùng làm baseline phiên này.

Tạo nhánh `refactor/lab4-pom` từ main tại `7ae637d78f16a68c8c67d2de539c6e46e72d569e`, giữ thay đổi chưa commit. Source đầu phiên đã có phần lớn POM; baseline là working tree đó, không phải HEAD sạch. Đã lưu source ZIP, hash từng file và patch so với HEAD trước khi sửa. PDF Lab 4 trong Downloads trùng hash bản trong DOCUMENT. Không thấy AGENTS.md trong repo hoặc D:/; file của Codex được kiểm tra là rỗng.

| Phần | Xác minh và hoàn thiện trong phiên này |
|---|---|
| A | BasePage abstract; Driver/Wait/Tim dùng chung; Page con kế thừa, không giữ driver/wait riêng; không Sleep/Delay/implicit wait |
| B | Locator private readonly; đủ API DangNhap/LayThongBaoLoi/DaHienThi/SapXep/LayDanhSachGia; sửa thêm OpenCart trả CartPage |
| C | Tests không có By; Pages không có Assert/NUnit hoặc public API trả IWebElement; CartTests nhận Page từ OpenCart; iframe dùng Tim(frame), giữ finally về DefaultContent |
| D | Giữ chaining DangNhap(...).SapXep("lohi").LayDanhSachGia(), đủ 6 giá và Is.Ordered.Ascending |
| E | Prompt, kết quả code và thẩm định AI hiện hành ghi trực tiếp trong README |

| Kiểm chứng mới | Kết quả |
|---|---|
| Build trước / sau | PASS; 0 error; NU1900 SSL khi audit NuGet |
| Trước sửa tiếp | 17 PASS, 0 FAIL, 37 giây; Explicit NotExecuted |
| Sau sửa | 17 PASS, 0 FAIL, 42 giây; Explicit NotExecuted |
| Explicit riêng | 1 FAIL dự kiến, NoSuchElementException tại #finish, 9 giây |
| Script đối chiếu | PASS: cùng mapping tên/outcome; đúng quy ước; hash data/config/framework không đổi |

Xem [diff riêng phiên này](evidence/lab4/session-20261114/session-code.diff), [TRX trước](evidence/lab4/session-20261114/before.trx), [TRX sau](evidence/lab4/session-20261114/after.trx), [log kiểm tra](evidence/lab4/session-20261114/checks.log). Ảnh mới: [trước](../screenshots/lab4-20261114-before.png), [sau](../screenshots/lab4-20261114-after.png). Đây là ảnh browser hiển thị log nguyên văn và bảng đọc từ TRX thật, có source/hash; không phải terminal/Test Explorer. Có thêm ảnh browser thật do test DynamicLoading PASS và ca lỗi cố ý đính kèm trong thư mục kết quả.

Chỗ chưa đúng của bản AI có sẵn: OpenCart trả void khiến test phải tự dựng CartPage, chưa theo quy tắc chuyển trang trả Page mới. Đã sửa cả Page và caller. Iframe còn lặp ID chuỗi dù đã có locator field; đã dùng Tim(frame). Assert trong LabPage và By trong test iframe đã được khắc phục trước phiên này, không nhận là lỗi mới sửa.

Giữ toàn bộ assert nghiệp vụ, JSON, Framework, NuGet và cấu hình. Không thêm Checkout, project test hoặc dependency; không sửa Word/form tài liệu. Đã commit/push và kiểm tra CI theo yêu cầu tiếp theo; file IDE cá nhân không đưa lên Git. Mỗi bộ test local chạy một lần. Các nhận định CI và prompt ở hồ sơ cũ dưới đây chỉ là tư liệu trước phiên.

## Hồ sơ có sẵn trước phiên 14/11/2026

Repo nộp: https://github.com/phuongvhn2009/KiemThu. Refactor trên project chung SauceDemo.Tests; không tạo project test mới, không tạo báo cáo Word. Đề gốc nằm trong DOCUMENT/02_11_2026___8ce62c9e-8bdd-453d-bdbf-59b20e4a9ecf.pdf và không bị chỉnh sửa.

## Đối chiếu yêu cầu

| Phần | Điểm | Trước refactor | Kết quả thực hiện |
|---|---:|---|---|
| A — Pages/Tests, namespace, BasePage | 20% | Có cấu trúc; Find tạo wait mỗi lần, LabPage tạo wait riêng | Giữ namespace SauceDemo.Tests.Pages/Tests; BasePage abstract chứa Driver, Wait và Tim(By); Page con dùng kế thừa |
| B — Locator và hành vi Page | 25% | Một số By nằm trực tiếp trong method; Login trả void; SortPrice không nhận giá trị | Mọi locator của Page con là private readonly By; DangNhap trả InventoryPage; LayThongBaoLoi; DaHienThi; SapXep(value) trả this; LayDanhSachGia trả IReadOnlyList<decimal> |
| C — Test chỉ dùng Page, hành vi không đổi | 30% | Iframe test chứa By, LabPage chứa Assert | Không còn chữ By trong Tests; Pages không có Assert/NUnit; không public API trả IWebElement; lưu ảnh/log/TRX trước và sau |
| D — Giá tăng dần, chaining | 15% | Test tự parse giá, gọi từng bước | DangNhap(...).SapXep("lohi").LayDanhSachGia(); Assert đủ 6 và Is.Ordered.Ascending |
| E — Thẩm định AI | 10% | Chưa có ghi nhận riêng Lab 4 | Prompt, đầu ra, lỗi quy ước và kiểm chứng ở phần dưới |

## Thiết kế và phạm vi thay đổi

- BasePage giữ driver, timeout và một WebDriverWait cho mỗi Page instance. Tim(By) chờ phần tử hiển thị và enabled, giữ xử lý NoSuchElementException/StaleElementReferenceException. Mọi Page kế thừa, không có driver/wait khai báo lại ở Page con.
- Tim là protected và trả IWebElement cho thao tác nội bộ. Quy tắc không trả IWebElement áp dụng API public của Page; nếu cấm helper protected thì sẽ mâu thuẫn chính yêu cầu hàm tìm phần tử của đề.
- LoginPage.DangNhap chỉ nhập tài khoản và submit rồi trả Page Object; không tự Assert hoặc chờ bắt buộc đăng nhập thành công. Điều này giữ ca tài khoản khóa, mật khẩu sai và ô trống hoạt động. Test đăng nhập đúng vẫn kiểm tra Products, URL /inventory.html và DaHienThi; test sai vẫn kiểm tra nguyên văn lỗi.
- InventoryPage giữ cách lấy danh sách qua FindElements và item.FindElement để đáp ứng Lab 2. Test tiếp tục in tên–giá, Assert 6 sản phẩm, tên không rỗng, tiền tố $, có Backpack. Parse decimal chuyển vào LayDanhSachGia dùng InvariantCulture.
- LabPage gom locator Start, finish, ba nút alert, result, iframe, editor và heading. OpenAlert trả string để test kiểm tra nội dung trước Accept/Dismiss; API xử lý hộp thoại tách riêng. Không trả IAlert hoặc IWebElement ra test.
- Iframe vẫn nhập chữ và kiểm tra nội dung; DefaultContent luôn nằm trong finally. Test đọc Heading qua Page để kiểm tra đã trở về trang ngoài.
- CartPage chỉ đổi helper Find sang Tim; test giỏ vẫn kiểm tra Backpack và quantity=1. Không thay config, gói NuGet, JSON fixture, browser, vòng đời BaseTest hoặc cơ chế chụp ảnh lỗi.
- DynamicLoadingWithoutWait vẫn là Explicit và cố ý gọi tìm ngay không wait. Đây là ngoại lệ có chủ đích của Lab 3, không đưa Tim vào nhánh này vì sẽ làm mất minh chứng lỗi đồng bộ.

## Ví dụ chaining thực tế

```csharp
var prices = Login.DangNhap("standard_user", "secret_sauce")
    .SapXep("lohi").LayDanhSachGia();
Assert.That(prices, Has.Count.EqualTo(6));
Assert.That(prices, Is.Ordered.Ascending);
```

Đăng nhập và dropdown ở SauceDemo. Dynamic loading, alert và iframe ở the-internet.herokuapp.com; không chuyển nhầm chức năng giữa hai website.

## Ghi nhận và thẩm định AI

AI thực hiện: Codex trong phiên làm việc trên repository. Yêu cầu người dùng: triển khai Lab 4 theo phần phân tích đã chốt, không làm Word, commit/push và ghi chi tiết. Quy ước refactor được xác định từ đề và dùng làm chỉ dẫn triển khai:

> Refactor các test Lab 2–3 trong SauceDemo.Tests sang POM, giữ tên test, dữ liệu và kiểm tra nghiệp vụ. BasePage abstract chứa driver, WebDriverWait dùng chung và Tim(By). Locator của Page con private readonly By. Không có By trong file test, không có Assert trong Page, API public không trả IWebElement. DangNhap trả InventoryPage; SapXep(value) trả this; LayDanhSachGia trả số và hỗ trợ chaining. Giữ ca đăng nhập sai và ca không chờ cố ý rớt. Chạy trước/sau, lưu bằng chứng và không tạo Word.

Đoạn trên ghi lại chỉ dẫn kỹ thuật đã áp dụng trong cùng phiên, không phải cuộc trò chuyện với dịch vụ AI khác. Đầu ra AI thực tế là các file trong Pages/, Tests/ và bản diff Git của commit Lab 4, không chỉ là đoạn code mẫu.

### Chỗ chưa đúng và cách sửa

1. Bản code trước Lab 4 do AI hỗ trợ có Assert.That(alert.Text, ...) trong LabPage.Alert. Điều này vi phạm việc Page chỉ mô tả thao tác/trạng thái. Đã thay bằng OpenAlert trả text, test Assert trước khi gọi AcceptAlert/DismissAlert/AnswerPrompt, rồi Assert Result. Không xóa mất một trong hai kiểm tra để làm test xanh.
2. Test Iframe trước đó tự dùng Driver.FindElement(By.TagName("h3")); locator bị lộ ra test. Đã thêm Heading trong LabPage, chuyển By thành field private readonly.
3. Locator inventory item/name/price và dropdown trước đó khai báo trực tiếp trong method; đã đưa thành field. LabPage cũng bỏ các WebDriverWait tự tạo để dùng Wait kế thừa.
4. Bản nháp refactor đầu tiên build có CS8603 vì IAlert.Text có thể null nhưng OpenAlert khai báo string. Đã sửa kiểu trả về thành string?; test vẫn Assert đúng chuỗi nên null không được tính PASS. Build cuối không còn cảnh báo C# này.

AI giúp tìm và sửa cấu trúc, nhưng code chạy xanh không tự chứng minh đủ quy ước POM. Vì vậy đã kiểm tra tĩnh Pages/Tests, đối chiếu tên và outcome từng test từ TRX, giữ đầy đủ Assert nghiệp vụ và chạy riêng ca cố ý rớt. Đặc biệt không chờ Inventory trong DangNhap cho mọi tài khoản, vì sẽ phá các ca login nghịch. Các kết quả dưới đây chỉ thuộc những lần thực thi đã lưu, không tuyên bố đã lặp 15–20 lần.

## Thực thi và bằng chứng

| Lần chạy | Kết quả thực tế | Bằng chứng |
|---|---|---|
| Trước refactor, một lần | 17 PASS, 0 FAIL, 49 giây | evidence/lab4/before.trx và before.log |
| Sau refactor, một lần | 17 PASS, 0 FAIL, 53 giây | evidence/lab4/after.trx và after.log |
| Ca Explicit sau refactor | 1 FAIL có chủ đích, 5 giây, NoSuchElementException tại finish | evidence/lab4/intentional-after.trx và intentional-after.log |
| Build cuối | Thành công, 0 error; còn NU1900 SSL khi audit NuGet | evidence/lab4/build.log |
| Kiểm tra quy ước và so sánh TRX | PASS | evidence/lab4/checks.log; Checks/check_lab4.py |

TRX của bộ thường lệ có 18 entry: 17 Passed và 1 NotExecuted cho Explicit; tổng executed do dotnet báo là 17. Script kiểm tra so sánh toàn bộ mapping tên–outcome trước/sau, kể cả Explicit chưa chạy. Số lượng giống nhau không thay thế việc đọc Assert, nên phạm vi bảo toàn hành vi đã nêu riêng ở trên.

Ảnh trước/sau: screenshots/lab4-before.png và screenshots/lab4-after.png. Đây là ảnh browser chụp trang hiển thị nguyên văn log thực thi trước/sau, được ghi rõ ngay trên ảnh; không giả giao diện terminal/Test Explorer. Log/TRX gốc đi kèm để đối chiếu. File HTML và profile chụp ảnh chỉ ở .artifacts, không đưa vào Git.

### Chạy lại trên Windows

```powershell
$env:CHROMEDRIVER_DIRECTORY = "$env:USERPROFILE/.cache/selenium/chromedriver/win64/154.0.8037.92"
# Chỉ dùng đường dẫn trên nếu driver còn khớp Cốc Cốc đang cài.
dotnet build SauceDemo.Tests --no-restore
dotnet test SauceDemo.Tests --logger 'trx;LogFileName=lab4.trx' --results-directory TestResults/lab4
# Lệnh sau phải rớt để minh chứng không chờ, không đưa vào bộ thường lệ.
dotnet test SauceDemo.Tests --filter 'Name=DynamicLoadingWithoutWait'
python Checks/check_lab4.py
```

Script Python chỉ dùng thư viện chuẩn và kiểm tra source hiện tại cùng các TRX đã lưu; không tự chạy browser. Nếu máy không có Python thì dùng Python 3 cài sẵn phù hợp. Cách chọn Cốc Cốc/Chrome và đường dẫn driver khác nằm trong README gốc.

## Giới hạn

Không tạo Word hoặc thêm dependency. Không thay PDF gốc, không tạo project trùng lặp. Chưa coi CI xanh hay thời gian 49/53 giây là kết luận hiệu năng: thời gian phụ thuộc mạng và máy. Cảnh báo NU1900 không bị tắt để che lỗi audit. Phiên bản source Lab 1–3 trong các báo cáo Word cũ là tư liệu lịch sử; source hiện tại và tài liệu Lab 4 này là căn cứ cho phần refactor.
