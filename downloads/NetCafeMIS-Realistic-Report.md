# Bộ dữ liệu quán net nhỏ và kết quả 30 truy vấn

Đây là **dữ liệu giả lập sát quy mô một quán nhỏ**, không phải số liệu thu thập từ một cơ sở kinh doanh thật. Thử trên bản `java.zip` mới nhất, giữ nguyên mã nguồn và kiểu dữ liệu của dự án.

## Quy mô

| Thành phần | Số liệu |
|---|---:|
| Máy | 20: 12 máy thường, 8 VIP |
| Giá máy thường / VIP | 12.000đ / 20.000đ mỗi giờ |
| Máy bảo trì tại thời điểm xem | 2 |
| Khách đăng ký | 40 |
| Nhân viên | 3 |
| Món ăn, đồ uống | 8, giá 8.000–35.000đ |
| Khoảng thời gian | 05–07/10/2026 |
| Phiên | 65: 61 hoàn tất, 4 đang chơi |
| Thời lượng phiên hoàn tất | 30 phút–3 giờ |
| Đơn món | 54 |
| Tổng phần món được gọi | 85 |
| Số phần lớn nhất trong một đơn | 3 |
| Hóa đơn | 59: 56 đã trả, 3 chưa trả |
| Phiên hoàn tất chờ lập hóa đơn | 2 |
| Thực thu | 2.592.000đ |
| Còn thu trên hóa đơn đã lập | 147.000đ |

Có khách quay lại, khách chưa chơi, máy chưa dùng, phiên qua nửa đêm, món hết hàng và món còn ít. Tất cả đơn được tạo qua Service với hàng đủ trong kho.

## Kết quả kiểm tra

- **30/30 truy vấn cho kết quả đúng** khi đối chiếu với sổ dữ liệu riêng.
- **66/66 lượt đối chiếu đúng**, gồm 30 câu chính và 36 lần đổi điều kiện thông thường: ngày, mã máy/khách/phiên, ngưỡng giá/tiền/tồn kho và thời điểm tạm tính.
- Câu 18 trả đồng hạng F02 (Mì trứng) và F07 (Snack), mỗi món 16 phần: đúng. Không có trường hợp vượt giới hạn int trong bộ này.
- Tiền máy, tiền món, hóa đơn, thực thu và công nợ được đối chiếu độc lập bằng Python Decimal/datetime từ cùng sổ giao dịch; Java chạy đúng Service và DAO của dự án.
- File Java demo đóng gói kèm cũng đã chạy, cho đúng cả 30 kết quả; menu khởi động và thoát đúng.
- Không đổi kiểu dữ liệu của mô hình. Các số lượng chỉ ở mức 1–3 phần mỗi đơn; tổng tiền của bộ dữ liệu cũng trong phạm vi int.

Thực thu theo ngày thanh toán:

| Ngày | Tiền |
|---|---:|
| 05/10/2026 | 908.000đ |
| 06/10/2026 | 946.000đ |
| 07/10/2026 | 738.000đ |

## Đọc và chạy

1. Mở `ket-qua-30-truy-van.txt` để xem đủ kết quả từng câu và điều kiện đã dùng.
2. Các CSV mở được bằng Excel: khách, máy, tồn món, phiên, đơn, hóa đơn và thanh toán. Ngày giờ và tiền đều là dữ liệu của bộ giả lập này.
3. Muốn chạy bằng IntelliJ: chép **RealisticCafeDemo.java** vào **src/main/java/netcafe/** của dự án NetCafeMIS hiện tại; bấm nút Run cạnh main(). File đã chứa toàn bộ dữ liệu, không cần thêm thư viện, Python hay database.
4. Chạy mặc định sẽ in 30 truy vấn theo các điều kiện trong bảng kết quả. Muốn vào menu để tự nhập điều kiện, thêm `--menu` trong Program arguments của cấu hình chạy RealisticCafeDemo.

Demo tạo NetCafe riêng trong bộ nhớ. Main.java và SampleData.java gốc không thay đổi. Thời điểm xem/tạm tính trong demo là **07/10/2026 23:00**; báo cáo theo ngày chính dùng 07/10/2026. Chạy lại sẽ tạo lại đúng bộ dữ liệu này.

## Kết quả từng câu

| Câu | Kết quả | Số dòng ở điều kiện chính |
|---|---|---:|
| Q01 | Đúng | 14 |
| Q02 | Đúng | 2 |
| Q03 | Đúng | 6 |
| Q04 | Đúng | 1 |
| Q05 | Đúng | 7 |
| Q06 | Đúng | 4 |
| Q07 | Đúng | 22 |
| Q08 | Đúng | 20 |
| Q09 | Đúng | 3 |
| Q10 | Đúng | 4 |
| Q11 | Đúng | 2 |
| Q12 | Đúng | 1 |
| Q13 | Đúng | 40 |
| Q14 | Đúng | 4 |
| Q15 | Đúng | 1 |
| Q16 | Đúng | 65 |
| Q17 | Đúng | 18 |
| Q18 | Đúng | 2 |
| Q19 | Đúng | 19 |
| Q20 | Đúng | 3 |
| Q21 | Đúng | 3 |
| Q22 | Đúng | 3 |
| Q23 | Đúng | 3 |
| Q24 | Đúng | 1 |
| Q25 | Đúng | 3 |
| Q26 | Đúng | 3 |
| Q27 | Đúng | 3 |
| Q28 | Đúng | 2 |
| Q29 | Đúng | 2 |
| Q30 | Đúng | 2 |

File `results.json` ghi expected và actual của cả 66 lượt, `summary.json` ghi số liệu tổng. `realistic-cafe.tsv` là sổ giao dịch dùng để kiểm tra, các dòng QUERY mô tả những điều kiện được thử. Bộ đóng gói này chỉ dùng dữ liệu quy mô quán nhỏ.
