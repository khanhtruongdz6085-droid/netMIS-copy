# Kết quả kiểm tra 30 truy vấn NetCafeMIS — 17/10/2026

Đối tượng: đúng mã nguồn trong `java.zip` mới nhất. Không thay đổi mã nguồn ứng dụng hoặc dữ liệu mẫu.

## Kết quả tổng

- 31 bộ dữ liệu: 1 bộ mẫu thật từ `SampleData.create()` và 30 bộ tự tạo (24 seed ngẫu nhiên và 6 bộ mục tiêu).
- 5.885 lượt đối chiếu kết quả DAO: **5.884 khớp, 1 sai**. Có 593 lượt đầu vào không hợp lệ được từ chối đúng; chúng được tính trong số khớp, không phải 593 truy vấn trả dữ liệu.
- Dữ liệu các bộ thử có tổng 1.990 phiên, 2.933 đơn món, 1.553 hóa đơn và 1.029 thanh toán. Đây là tổng giữa các bộ dữ liệu, không phải toàn bộ được thêm vào dữ liệu mẫu của dự án.
- Đường chạy thực tế `Main --all`: **30/30 kết quả khớp** với tính toán đối chiếu trên dữ liệu mẫu.
- Các ca kiểm tra có theo dõi số bản ghi, tồn kho và trạng thái trước/sau truy vấn; không thấy truy vấn làm thay đổi dữ liệu.

## Lỗi xác nhận: Q18 — Món có số lượng gọi cao nhất

Tái hiện hoàn toàn qua Service với dữ liệu hợp lệ, xử lý tuần tự:

1. F01 được nhập đủ hàng và gọi 2.147.483.647 món.
2. Nhập thêm 1 F01 và gọi thêm 1. Tổng F01 = **2.147.483.648**.
3. F02 chỉ được gọi **1** món.

Kết quả mong đợi:

```text
F01 | 2147483648
```

Kết quả thực tế:

```text
F02 | 1
```

Nguyên nhân trong `FoodDao.monDuocGoiNhieuNhat()`: `Collectors.summingInt(...)` và `Map<Food, Integer>` khiến tổng tràn số. Bản thân từng đơn vẫn nằm trong giới hạn int. Đề xuất đổi phần cộng tổng sang `long` / `summingLong()`; chưa áp dụng sửa.

## Kết quả từng câu

Số lượt khớp bao gồm cả các ca từ chối đầu vào sai đúng yêu cầu. Kết luận “chưa thấy lỗi” chỉ áp dụng các bộ dữ liệu và điều kiện được thử, không phải chứng minh đúng với mọi đầu vào.

| Câu | Nội dung | Khớp | Sai | Kết luận |
|---|---|---:|---:|---|
| Q01 | Máy trống, khu và giá giờ | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q02 | Máy bảo trì và khu | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q03 | Số máy theo khu và trạng thái | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q04 | Khu có giá > ngưỡng nhập | 383 | 0 | Chưa thấy lỗi trong bộ thử |
| Q05 | Lịch sử phiên theo mã máy | 437 | 0 | Chưa thấy lỗi trong bộ thử |
| Q06 | Phiên đang hoạt động | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q07 | Phiên hoàn tất >= 120 phút | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q08 | Phút chơi theo máy trong ngày nhập | 393 | 0 | Chưa thấy lỗi trong bộ thử |
| Q09 | Máy chưa có phiên | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q10 | Tạm tính phiên đang chơi theo thời điểm nhập | 357 | 0 | Chưa thấy lỗi trong bộ thử |
| Q11 | Lịch sử phiên theo mã khách | 437 | 0 | Chưa thấy lỗi trong bộ thử |
| Q12 | Khách chưa có phiên | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q13 | Số phiên hoàn tất theo khách | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q14 | Món có giá <= ngưỡng nhập | 383 | 0 | Chưa thấy lỗi trong bộ thử |
| Q15 | Chi tiết gọi món theo mã phiên | 421 | 0 | Chưa thấy lỗi trong bộ thử |
| Q16 | Tiền món theo phiên | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q17 | Phiên hoàn tất không gọi món | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q18 | Món có số lượng gọi cao nhất | 103 | 1 | Lỗi tràn tổng số lượng |
| Q19 | Lần gọi món > ngưỡng nhập | 383 | 0 | Chưa thấy lỗi trong bộ thử |
| Q20 | Nhân viên và đơn món theo ngày nhập | 321 | 0 | Chưa thấy lỗi trong bộ thử |
| Q21 | Hóa đơn chưa thanh toán | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q22 | Doanh thu theo ngày thanh toán | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q23 | Giao dịch theo phương thức | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q24 | Hóa đơn lớn nhất | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q25 | Top 3 khách đã trả nhiều nhất | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q26 | Giá trị hóa đơn theo nhân viên lập | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q27 | Tiền còn thu theo khách | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q28 | Doanh thu theo khu máy | 104 | 0 | Chưa thấy lỗi trong bộ thử |
| Q29 | Món có tồn kho <= ngưỡng nhập | 290 | 0 | Chưa thấy lỗi trong bộ thử |
| Q30 | Phiên hoàn tất chưa có hóa đơn | 104 | 0 | Chưa thấy lỗi trong bộ thử |

## Phạm vi và cách đối chiếu

- Quán hoàn toàn rỗng; có danh mục nhưng chưa có phiên; đối tượng chưa từng được dùng và nhóm có tổng bằng 0.
- Phiên đang hoạt động, phiên hoàn tất, phiên chưa có hóa đơn; hóa đơn đã trả/chưa trả và giá trị 0; đủ ba phương thức thanh toán.
- Khách trùng tên nhưng khác mã; nhiều kết quả đồng hạng; bốn khách bằng tiền kiểm tra top 3 và tiêu chí phụ theo mã; nhiều món cùng đứng đầu.
- Giá bằng đúng ngưỡng, giá thấp/cao hơn ngưỡng, giá lẻ, món miễn phí, giá trị tiền lớn; tồn 0, tồn đúng ngưỡng và ngưỡng int tối đa.
- Phiên 119 phút 59 giây, 120 phút, 120 phút 1 giây, phiên qua nửa đêm, qua ngày nhuận; thanh toán khác ngày chơi; ngày không có hoạt động; thời điểm tạm tính trước/sau giờ mở các phiên active.
- ID đúng, dư khoảng trắng, không tồn tại, rỗng và null; ngưỡng âm/null; ngày/thời điểm null (khi kiểu API cho phép truyền null).
- Dữ liệu ngẫu nhiên được kiểm tra cả sau 1, 20, 40 phiên và khi hoàn tất, để truy vấn thấy cập nhật mới.

`QueryAuditDriver.java` chỉ tạo dữ liệu qua các Service và lấy kết quả thật từ DAO. `run_audit.py` giữ sổ sự kiện riêng, dùng vòng lặp Python, số nguyên không giới hạn, Decimal và datetime để tính kết quả mong đợi. Nó không gọi DAO để tạo expected. So sánh cả nội dung từng dòng và thứ tự. Q28 đối chiếu tổng thực thu trực tiếp từ các thanh toán. Q8 phân bổ thời gian vào từng ngày trước khi tổng hợp.

Các mốc được xác nhận thêm bằng số cố định: mẫu gốc thu 52.000đ ngày 05/10 và 77.000đ ngày 06/10, tổng 129.000đ; bộ đồng hạng giữ đủ hai món cùng số lượng 4; bộ rỗng vẫn có ba dòng phương thức thanh toán với số lượng/tiền bằng 0.

Phần thử này tập trung truy vấn trên dữ liệu được tạo hợp lệ, tuần tự; không coi những lỗi ghi dữ liệu đồng thời đã báo trước là lỗi mới của 30 câu truy vấn. Thời gian đầu vào dùng độ chính xác giây, tương thích với menu nhập tới phút; không đưa các trường hợp dưới giây vào kết luận này.

## Chạy lại bộ test

Cần Python 3.10+ và JDK 21+. Giải nén bộ test rồi chạy trong terminal:

```text
python run_audit.py --project "DUONG_DAN_THU_MUC_NetCafeMIS" --java-home "DUONG_DAN_THU_MUC_JDK"
```

Ví dụ PowerShell: `--project "C:\Users\THUY\OneDrive\Desktop\NetCafeMIS"`. `--java-home` trỏ tới thư mục JDK, không phải file java.exe hoặc thư mục bin.

Với mã nguồn hiện tại, chương trình **trả exit code 1 vì có một ca Q18 sai**. Đây là kết quả phát hiện lỗi, không phải bộ test chạy thành công toàn bộ. Sau khi sửa đúng Q18, kỳ vọng exit code 0; không bỏ qua hay đổi kết quả mong đợi để làm test xanh.

Các file đi kèm: 31 sổ sự kiện trong datasets/, expected.json, actual.tsv (dòng kết quả được mã hóa Base64 để giữ nguyên UTF-8), results.json, summary.tsv, run-metadata.json và main-all.txt.
