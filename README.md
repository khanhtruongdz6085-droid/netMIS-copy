# NetCafeMIS — dữ liệu trong bộ nhớ và 30 truy vấn DAO

Project Java 8 chạy console. `Main.sample()` tự tạo dữ liệu mẫu bằng các đối tượng Java; service ghi dữ liệu vào các `List<T>` dùng chung. Các DAO đọc chính những list đó bằng Stream/Lambda. Không cần dịch vụ ngoài hoặc thư viện chạy bổ sung.

## Mở bằng IntelliJ IDEA

1. Mở thư mục project hoặc `pom.xml` như Maven project; chọn JDK 8 trở lên và UTF-8.
2. Chạy `netcafe.Main` để xem dữ liệu mẫu.
3. Muốn chạy 30 truy vấn, thêm **Program arguments: `--queries`** cho Main.
4. Chạy `netcafe.Checks.QueryChecks` để kiểm tra các truy vấn.

Dữ liệu mẫu có hai khu, sáu máy, bốn khách/tài khoản, ba nhân viên, ba món, bốn phiên, ba lần gọi món, hai hóa đơn và một thanh toán. S04 còn ACTIVE; R01 = 49000 đã trả, R02 = 60000 chưa trả. Chạy lại Main sẽ tạo lại dữ liệu từ đầu.

## Chạy bằng Maven

```sh
mvn clean package
java -jar target/netcafe-mis-1.0.0.jar
java -jar target/netcafe-mis-1.0.0.jar --queries
java -cp target/classes:target/test-classes netcafe.Checks.QueryChecks
```

Trên Windows, thay dấu `:` giữa hai thư mục classpath bằng `;`. Với JDK 9 trở lên, thêm `-Dmaven.compiler.release=8` khi build để kiểm tra tương thích Java 8.

Các kiểm tra là chương trình main độc lập; Maven Surefire không tự chạy chúng:

```sh
java -cp target/classes:target/test-classes netcafe.Checks.NetCafeChecks
java -cp target/classes:target/test-classes netcafe.Checks.DiscountChecks
java -cp target/classes:target/test-classes netcafe.Checks.AccountChecks
java -cp target/classes:target/test-classes netcafe.Checks.EarlyEndChecks
java -cp target/classes:target/test-classes netcafe.Checks.SessionEndChecks
java -cp target/classes:target/test-classes netcafe.Checks.QueryChecks
```

Có thể chạy các lớp `netcafe.Demo.DiscountDemo`, `netcafe.Demo.ExceptionDemo`, `netcafe.Demo.EarlyEndDemo` với cùng classpath sau khi build.

## Chạy bằng PowerShell, chỉ cần JDK

```powershell
.\run.ps1
.\run.ps1 -Queries
.\run.ps1 -QueryChecks
.\run.ps1 -Checks
.\run.ps1 -DiscountChecks
.\run.ps1 -AccountChecks
.\run.ps1 -EarlyEndChecks
.\run.ps1 -SessionEndChecks
.\run.ps1 -DiscountDemo
.\run.ps1 -ExceptionDemo
.\run.ps1 -EarlyEndDemo
```

Script tìm JDK từ JAVA_HOME, PATH hoặc JDK đi kèm IntelliJ. Có thể truyền `-JavaHome 'C:\path\to\jdk'`. Nó biên dịch cả mã chính, demo và kiểm tra bằng javac.

## Luồng dữ liệu

```text
Main.sample() ── gọi Service ──> các List trong NetCafeData
                                      ↑          ↑
                              Service cập nhật   DAO truy vấn
                                      ↑          ↑
                                      Main gọi nghiệp vụ/truy vấn
```

`NetCafe` tạo cả service lẫn DAO và truyền cùng backing list cho chúng. DAO không tạo lại khu, khách hoặc các thực thể liên quan. Vì vậy kết quả giữ đúng tham chiếu đối tượng đã đăng ký; những thay đổi hợp lệ từ service được nhìn thấy ngay khi truy vấn lại.

- Tạo/sửa nghiệp vụ qua service hoặc các phương thức của `NetCafe`.
- Đọc và thống kê qua `cafe.getComputerDAO()`, `getSessionDAO()`, `getReceiptDAO()`…
- `DAO.findAll()` là view chỉ đọc của list đang dùng. Kết quả truy vấn là list chỉ đọc tại thời điểm gọi; các thực thể bên trong vẫn là đối tượng của quán.
- DAO không có `insert/update/delete` công khai để bỏ qua kiểm tra nghiệp vụ.
- Chưa lưu ra file; dừng chương trình sẽ mất những thay đổi trong lần chạy đó.

```java
NetCafe cafe = Main.sample();
cafe.getComputerDAO().findAvailable().forEach(computer ->
    System.out.println(computer.getId() + " " + computer.getSection().getSectionName()));

cafe.getReceiptDAO().findUnpaid().forEach(receipt ->
    System.out.println(receipt.getId() + " " + receipt.getTotalAmount()));

System.out.println(cafe.getPaymentDAO().collectedAmount()); // 49000
```

## Nơi xem 30 truy vấn

- [TRUY_VAN_30_JAVA.md](TRUY_VAN_30_JAVA.md): đánh số Q01–Q30, vị trí code, tham số và kết quả mẫu.
- [QueryDemo.java](src/main/java/netcafe/QueryDemo.java): chạy và in cả 30 truy vấn trên dữ liệu của Main.
- [dao/](src/main/java/netcafe/dao): Stream/Lambda thực thi truy vấn.
- [QueryResults.java](src/main/java/netcafe/dao/QueryResults.java): các lớp kết quả tổng hợp, có getter rõ kiểu dữ liệu.
- [EXPECTED_QUERY_OUTPUT.txt](EXPECTED_QUERY_OUTPUT.txt): kết quả đầy đủ của `Main --queries`.
- [QueryChecks.java](src/test/java/netcafe/Checks/QueryChecks.java): kiểm tra đủ 30 câu và các trường hợp biên.

## Cấu trúc nguồn

```text
src/main/java/netcafe/
├── Main.java                 # Tạo mẫu, in tóm tắt hoặc chọn --queries
├── QueryDemo.java            # In Q01–Q30
├── dao/                      # 10 DAO, ListDAO và QueryResults
├── Service/                  # NetCafe, dữ liệu chung, các service và FoodService
├── model/
│   ├── NetSpecs/             # ComputerSection, Computer, Employee, Food
│   ├── customer/             # Customer và Normal/Member/VipCustomer
│   └── ...                   # CustomerAccount, Session, Receipt, Payment, chi tiết món
├── enums/
└── exception/

src/test/java/netcafe/
├── Checks/                   # Sáu chương trình kiểm tra
└── Demo/                     # Ba demo nghiệp vụ
```

## Nghiệp vụ được giữ lại

Mở phiên bằng tài khoản ACTIVE và máy AVAILABLE; kiểm tra lịch sử và phiên đang chạy. Ghi nhận món theo giá tại lúc gọi, gộp món cùng giá và khóa đơn. `Session.endSession()` kiểm tra trạng thái, giờ kết thúc và các lần gọi món trước khi chuyển phiên COMPLETED/máy AVAILABLE. Gọi qua service còn kiểm tra phiên thuộc quán. Giờ kết thúc là giờ nghỉ thực tế; chưa đặt giờ kết thúc dự kiến.

Normal giảm 0%; Member giảm 10% tiền máy, 5% tiền món; VIP giảm 15% tiền máy, 8% tiền món. Hóa đơn chốt tiền gốc, tỷ lệ giảm và tiền sau giảm. Thanh toán đủ một lần, không trước giờ kết thúc. Thực thu lấy từ Payment, không cộng mọi hóa đơn.

Tài khoản giữ username, hash, số dư và trạng thái. Chưa có xác thực đăng nhập, nạp/trừ tiền hoặc nâng hạng khách. Hash trong mẫu chỉ là placeholder. Xem thêm [hướng dẫn nghiệp vụ](HUONG_DAN_NGHIEP_VU.md) và [đặc tả](Dac_ta_quan_ly_quan_net.md).

## Kiểm chứng

Build với Maven 3.9.9/JDK 21 và `--release 8` thành công. Sáu chương trình kiểm tra: 43 + 42 + 54 + 40 + 132 + 169 = **480 kiểm tra**. Kết quả Main khớp `EXPECTED_OUTPUT.txt`; kết quả 30 truy vấn khớp `EXPECTED_QUERY_OUTPUT.txt`. Phần truy vấn đã kiểm tra dữ liệu rỗng, kết quả đồng hạng, mốc ngày, nhiều tài khoản của cùng khách, cập nhật từ service, giá lịch sử và thanh toán nhập không nhất quán.
