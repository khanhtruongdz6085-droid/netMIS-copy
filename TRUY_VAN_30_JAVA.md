# 30 truy vấn Java Stream/Lambda qua DAO

Đây là phần triển khai đúng 30 yêu cầu ở mục 4 của [đặc tả](Dac_ta_quan_ly_quan_net.md). Các DAO chỉ đọc các `List<T>` dùng chung với service. Nguồn dữ liệu được tạo trong `Main.sample()`; không có nguồn dữ liệu ngoài.

## Chạy toàn bộ

```sh
mvn clean package
java -jar target/netcafe-mis-1.0.0.jar --queries
```

Trong IntelliJ: chọn `netcafe.Main`, điền Program arguments `--queries`. Hoặc PowerShell: `.\run.ps1 -Queries`.

[QueryDemo.java](src/main/java/netcafe/QueryDemo.java) gọi lần lượt Q01–Q30, chỉ đọc dữ liệu. [EXPECTED_QUERY_OUTPUT.txt](EXPECTED_QUERY_OUTPUT.txt) chứa toàn bộ kết quả console để đối chiếu. Các ngưỡng/mã/ngày minh họa có thể đổi tại lời gọi DAO.

## Ánh xạ yêu cầu tới code

Trong các lời gọi dưới đây: `NetCafe cafe = Main.sample();` và `LocalDate day = LocalDate.of(2026, 10, 5);`. Ví dụ hàng Q01 gọi `cafe.getComputerDAO().findAvailable()`.

### Thành viên 1 — Khu, máy và phiên

| Câu | Yêu cầu | DAO và phương thức | Kết quả mẫu |
|---|---|---|---|
| Q01 | Máy AVAILABLE, giá giờ tăng dần | [ComputerDAO](src/main/java/netcafe/dao/ComputerDAO.java#L17): `findAvailable()` | PC01, PC03, PC04, PC06 |
| Q02 | Máy MAINTENANCE | [ComputerDAO](src/main/java/netcafe/dao/ComputerDAO.java#L24): `findMaintenance()` | PC05 |
| Q03 | Số máy theo từng trạng thái trong mỗi khu | [ComputerSectionDAO](src/main/java/netcafe/dao/ComputerSectionDAO.java#L21): `countComputersByStatus()` | CS01: 1 trống/1 dùng/1 bảo trì; CS02: 3 trống/0/0 |
| Q04 | Khu có giá giờ lớn hơn ngưỡng, giá giảm dần | [ComputerSectionDAO](src/main/java/netcafe/dao/ComputerSectionDAO.java#L31): `findAboveHourlyPrice(new BigDecimal("15000"))` | CS02 = 20000 |
| Q05 | Lịch sử của máy, phiên mới nhất trước | [SessionDAO](src/main/java/netcafe/dao/SessionDAO.java#L40): `historyByComputer("PC01")` | S01, khách C01, 08:00–10:00 |
| Q06 | Phiên ACTIVE, khách, máy và khu | [SessionDAO](src/main/java/netcafe/dao/SessionDAO.java#L46): `findActive()` | S04, C01, PC02, CS01 |
| Q07 | Phiên COMPLETED ít nhất 120 phút | [SessionDAO](src/main/java/netcafe/dao/SessionDAO.java#L56): `findCompletedAtLeastMinutes(120)` | S01 = 120 phút |
| Q08 | Tổng phút phiên COMPLETED bắt đầu trong ngày, mọi máy | [SessionDAO](src/main/java/netcafe/dao/SessionDAO.java#L64): `completedMinutesByComputerOn(day)` | PC01=120, PC02=0, PC03=90, PC04=60, PC05=0, PC06=0 |
| Q09 | Máy chưa từng có phiên | [ComputerDAO](src/main/java/netcafe/dao/ComputerDAO.java#L30): `findNeverUsed()` | PC05, PC06 |
| Q10 | Tạm tính tiền máy ACTIVE tại thời điểm nhập | [SessionDAO](src/main/java/netcafe/dao/SessionDAO.java#L76): `estimateActiveAmounts(day.atTime(12, 0))` | S04 = 12000 |

### Thành viên 2 — Khách, nhân viên và gọi món

| Câu | Yêu cầu | DAO và phương thức | Kết quả mẫu |
|---|---|---|---|
| Q11 | Lịch sử khách qua mọi tài khoản, mới nhất trước | [SessionDAO](src/main/java/netcafe/dao/SessionDAO.java#L83): `historyByCustomer("C01")` | S04, S01 |
| Q12 | Khách chưa từng có phiên | [CustomerDAO](src/main/java/netcafe/dao/CustomerDAO.java#L33): `findNeverPlayed()` | C04 |
| Q13 | Đếm phiên COMPLETED theo mọi khách | [CustomerDAO](src/main/java/netcafe/dao/CustomerDAO.java#L40): `countCompletedSessions()` | C01=1, C02=1, C03=1, C04=0 |
| Q14 | Món có giá hiện tại không vượt ngưỡng | [FoodDAO](src/main/java/netcafe/dao/FoodDAO.java#L23): `findAtMostPrice(new BigDecimal("15000"))` | F03=7000, F01=13000, F02=15000 |
| Q15 | Đơn, nhân viên, món và giá lịch sử của phiên | [FoodServiceDAO](src/main/java/netcafe/dao/FoodServiceDAO.java#L23): `orderDetailsBySession("S01")` | SV01: F01 × 1 @15000, F02 × 1 @10000; nhân viên E01 |
| Q16 | Tổng tiền món gốc từng phiên, kể cả 0 | [FoodServiceDAO](src/main/java/netcafe/dao/FoodServiceDAO.java#L32): `totalsBySession()` | S02=30000, S01=25000, S04=20000, S03=0 |
| Q17 | Phiên COMPLETED không gọi món | [SessionDAO](src/main/java/netcafe/dao/SessionDAO.java#L90): `findCompletedWithoutFood()` | S03 |
| Q18 | Món được gọi nhiều nhất, giữ mọi đồng hạng | [FoodDAO](src/main/java/netcafe/dao/FoodDAO.java#L30): `findMostOrdered()` | F01 và F02 cùng 3 phần |
| Q19 | Lần gọi món có tổng tiền lớn hơn ngưỡng | [FoodServiceDAO](src/main/java/netcafe/dao/FoodServiceDAO.java#L41): `findAboveTotal(new BigDecimal("20000"))` | SV02=30000, SV01=25000 |
| Q20 | Số lần gọi và tiền món theo mọi nhân viên/ngày | [EmployeeDAO](src/main/java/netcafe/dao/EmployeeDAO.java#L32): `foodStatsOn(day)` | E01: 2/45000; E02: 1/30000; E03: 0/0 |

### Thành viên 3 — Hóa đơn, thanh toán và thống kê

| Câu | Yêu cầu | DAO và phương thức | Kết quả mẫu |
|---|---|---|---|
| Q21 | Hóa đơn chưa thanh toán, kèm khách/nhân viên | [ReceiptDAO](src/main/java/netcafe/dao/ReceiptDAO.java#L24): `findUnpaid()` | R02, khách C02, nhân viên E01, 60000 |
| Q22 | Thực thu theo ngày thanh toán | [PaymentDAO](src/main/java/netcafe/dao/PaymentDAO.java#L34): `revenueByDay()` | 2026-10-05 = 49000 |
| Q23 | Số giao dịch và tiền theo mọi phương thức | [PaymentDAO](src/main/java/netcafe/dao/PaymentDAO.java#L41): `revenueByMethod()` | CASH: 1/49000; BANK_TRANSFER, CARD: 0/0 |
| Q24 | Hóa đơn có tổng lớn nhất, giữ đồng hạng | [ReceiptDAO](src/main/java/netcafe/dao/ReceiptDAO.java#L30): `findLargest()` | R02 = 60000 |
| Q25 | Tối đa 3 khách đã thanh toán, tổng giảm rồi mã tăng | [CustomerDAO](src/main/java/netcafe/dao/CustomerDAO.java#L49): `topThreePayingCustomers()` | C01 = 49000 |
| Q26 | Thống kê hóa đơn theo mọi nhân viên lập | [EmployeeDAO](src/main/java/netcafe/dao/EmployeeDAO.java#L45): `billingStats()` | E01: 1/60000/thu 0/nợ 60000; E02: 1/49000/thu 49000/nợ 0; E03: 0 |
| Q27 | Tổng còn phải thu theo khách có hóa đơn chưa trả | [CustomerDAO](src/main/java/netcafe/dao/CustomerDAO.java#L60): `outstandingByCustomer()` | C02 = 60000 |
| Q28 | Thực thu theo mọi khu, tách tiền máy/món sau giảm | [PaymentDAO](src/main/java/netcafe/dao/PaymentDAO.java#L50): `revenueBySection()` | CS01: máy 24000 + món 25000 = 49000; CS02 = 0 |
| Q29 | Phát hiện nhiều thanh toán hoặc thanh toán khác tổng | [ReceiptDAO](src/main/java/netcafe/dao/ReceiptDAO.java#L37): `findPaymentIssues()` | Rỗng vì dữ liệu mẫu hợp lệ |
| Q30 | Phiên COMPLETED chưa có hóa đơn | [SessionDAO](src/main/java/netcafe/dao/SessionDAO.java#L97): `findCompletedWithoutReceipt()` | S03 |

## Ví dụ sử dụng trong Main

```java
NetCafe cafe = Main.sample();

// Q05: Lấy khách từ đúng tham chiếu của phiên.
cafe.getSessionDAO().historyByComputer("PC01").forEach(session -> {
    System.out.println(session.getId());
    System.out.println(session.getAccount().getCustomer().getName());
});

// Q13: Các kết quả tổng hợp có kiểu và getter rõ ràng.
cafe.getCustomerDAO().countCompletedSessions().forEach(row -> {
    System.out.println(row.getCustomer().getName() + ": " + row.getCount());
});

// Q28: Tiền đã thu, tách máy và món sau giảm.
cafe.getPaymentDAO().revenueBySection().forEach(row -> {
    System.out.println(row.getSection().getSectionName());
    System.out.println("Máy=" + row.getComputerAmount());
    System.out.println("Món=" + row.getFoodAmount());
    System.out.println("Thực thu=" + row.getCollected());
});
```

Các lớp kết quả tổng hợp nằm trong [QueryResults.java](src/main/java/netcafe/dao/QueryResults.java). Các truy vấn trả thực thể thì giữ nguyên đối tượng của quán; không tạo lại khách/khu/máy khi đọc. `findById()` dùng được trên cả 10 DAO; `CustomerAccountDAO.findByUsername()` tìm tên đăng nhập không phân biệt hoa/thường và bỏ khoảng trắng hai đầu.

## Quy tắc kết quả

- Các truy vấn trả list chỉ đọc. `findAll()` giữ view đang dùng; kết quả lọc/tổng hợp là snapshot của lần truy vấn, cần gọi lại để thấy thay đổi mới.
- Không có kết quả thì trả list rỗng; tổng và số lượng bằng 0. Q03/Q08/Q13/Q16/Q20/Q23/Q26/Q28 vẫn giữ các khu/máy/khách/nhân viên/phương thức có kết quả 0.
- Giá giờ/món/ngưỡng dùng `BigDecimal`; so sánh bằng `compareTo`, không dùng double hoặc equals phụ thuộc số chữ số sau dấu phẩy.
- Q07/Q08 dùng số phút nguyên của từng phiên, không làm tròn lên. Q08 xét ngày **bắt đầu** của phiên hoàn tất, không chia thời gian một phiên ra nhiều ngày.
- Q10 gọi `Session.amountAt(at)`. Nếu thời điểm nhập trước giờ bắt đầu của bất kỳ phiên ACTIVE nào, truy vấn báo `InvalidTimeException`; không lặng lẽ bỏ qua phiên đó. Không kết thúc phiên hoặc thay đổi trạng thái.
- Q11/Q12/Q13/Q25/Q27 thống kê theo khách qua tất cả tài khoản của khách đó.
- Q15/Q16/Q18/Q19/Q20 lấy các đơn đã ghi nhận, kể cả phiên ACTIVE. Q15/Q16/Q19/Q20 dùng giá lúc gọi, không lấy giá món hiện tại và chưa trừ ưu đãi hóa đơn.
- Q18/Q24 giữ tất cả kết quả đồng hạng. Q25 chỉ lấy tối đa ba khách có Payment, kể cả giao dịch hợp lệ giá trị 0; tổng giảm dần rồi mã khách tăng dần.
- Q20 xét ngày gọi món. Q22 xét ngày **thanh toán**, không phải ngày bắt đầu/kết thúc phiên. Khoảng ngày luôn gồm 00:00 đầu ngày, loại 00:00 ngày tiếp theo.
- Q26 phân theo **nhân viên lập hóa đơn**, không phải nhân viên xử lý món.
- Q28 dùng tiền máy/món **sau giảm** của hóa đơn đã thanh toán. Các khoản của mỗi hóa đơn được cộng một lần; thực thu cộng Payment.Amount. Khi Q29 báo lỗi thì cần đối soát trước khi dùng số liệu doanh thu.
- Q29 không xem hóa đơn chưa thanh toán là lỗi. Nó trả một hoặc cả hai lý do `MULTIPLE_PAYMENTS`, `AMOUNT_MISMATCH`. Luồng service chặn các lỗi này, nên mẫu trả `[]` là đúng. QueryChecks dùng một danh sách nhập sai riêng, không sửa dữ liệu hoặc nới lỏng quy tắc nghiệp vụ, để kiểm tra khả năng phát hiện.

## Kiểm tra

```sh
java -cp target/classes:target/test-classes netcafe.Checks.QueryChecks
```

Windows dùng `;` trong classpath, hoặc `.\run.ps1 -QueryChecks`. Bộ này có 169 kiểm tra: đủ Q01–Q30 trên mẫu, tất cả truy vấn trên dữ liệu rỗng, mốc ngày, đồng hạng, nhiều tài khoản/khách, nguồn dữ liệu chung và các lỗi nhập thanh toán. Năm bộ kiểm tra nghiệp vụ có sẵn vẫn được giữ lại.
