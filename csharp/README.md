# NetCafeMIS — phiên bản C#

Bản console C# chuyển từ bản Java đã sửa lỗi kết thúc phiên, commit `ab383292d590b59135735dd069f693c50d9ea636`. Bản Java ở thư mục `src/` được giữ lại. Mỗi lớp, interface và enum C# có file riêng.

Xem [sơ đồ quan hệ, PK/FK và ví dụ LINQ](SO_DO_QUAN_HE_LINQ.md) để xác định nguồn dữ liệu và đường liên kết giữa các model.

## Mở và chạy

Cài **.NET 8 SDK**. Với Visual Studio 2022, dùng phiên bản 17.8 trở lên, cài workload **.NET desktop development**, rồi mở `NetCafeMis.sln`. Chọn `NetCafeMis` làm startup project để chạy dữ liệu mẫu, hoặc `NetCafeMis.Checks` để chạy kiểm tra.

Chạy bằng terminal từ thư mục `csharp/`:

```sh
dotnet build NetCafeMis.sln -c Release
dotnet run --project NetCafeMis -c Release --no-build
dotnet run --project NetCafeMis.Checks -c Release --no-build
```

Project không dùng thư viện NuGet bên ngoài. `NuGet.Config` để trống nguồn package vì chỉ cần framework có sẵn trong SDK. `global.json` chọn SDK thuộc dòng .NET 8.

Các demo:

```sh
dotnet run --project NetCafeMis -c Release --no-build -- discount
dotnet run --project NetCafeMis -c Release --no-build -- early-end
dotnet run --project NetCafeMis -c Release --no-build -- exceptions
```

Các nhóm kiểm tra có thể chạy riêng bằng tham số `business`, `discount`, `accounts`, `early-end`, `session-end` hoặc `csharp` của project `NetCafeMis.Checks`. Mặc định chạy tất cả. Đây là chương trình kiểm tra độc lập; dùng `dotnet run`, không dùng `dotnet test` để chạy chúng.

## Cấu trúc

```text
csharp/
├── NetCafeMis.sln
├── global.json
├── NuGet.Config
├── NetCafeMis/
│   ├── Program.cs                 # Chọn dữ liệu mẫu hoặc demo
│   ├── SampleData.cs              # Dữ liệu mẫu giống Java Main.sample()
│   ├── IIdentifiable.cs
│   ├── IChargeable.cs
│   ├── IDiscount.cs
│   ├── Models/                    # Session, CustomerAccount, Receipt, Payment…
│   │   └── Customers/             # NormalCustomer, MemberCustomer, VipCustomer
│   ├── Services/                  # Catalog, Session, FoodOrder, Receipt, Payment
│   ├── Enums/
│   ├── Exceptions/                # 14 loại lỗi nghiệp vụ
│   └── Demos/
└── NetCafeMis.Checks/              # Kiểm tra nghiệp vụ và tương thích C#
```

## Nghiệp vụ được giữ lại

1. Đăng ký khu, máy, khách, tài khoản, nhân viên và món. Mã không trùng; username không phân biệt hoa/thường; chủ tài khoản phải được đăng ký.
2. Mở phiên bằng tài khoản ACTIVE và máy AVAILABLE; kiểm tra phiên đang chạy và lịch sử theo tài khoản/máy.
3. Gọi món trên phiên ACTIVE với nhân viên ACTIVE; giữ giá tại lúc gọi, gộp cùng món/cùng giá và khóa đơn sau khi ghi nhận.
4. Kết thúc theo giờ nghỉ thực tế. Không lưu giờ kết thúc dự kiến.
5. Lập một hóa đơn cho phiên COMPLETED; chốt tiền gốc, tỷ lệ giảm, tiền giảm và tiền phải trả.
6. Thanh toán đủ một lần; thời gian thanh toán không trước giờ kết thúc. Doanh thu thực thu là tổng tiền của các Payment.

| Hạng khách | Giảm tiền máy | Giảm tiền món |
|---|---:|---:|
| Normal | 0% | 0% |
| Member | 10% | 5% |
| VIP | 15% | 8% |

`Session` giữ các đơn đã khóa, và danh sách bên ngoài chỉ đọc. Cả `cafe.EndSession(session, end)` lẫn `session.EndSession(end)` đều kiểm tra trạng thái, giờ bắt đầu và các lần gọi món trước khi cập nhật phiên/máy. Service còn kiểm tra phiên thuộc quán.

Ví dụ S04 bắt đầu 11:00, gọi món 11:15: kết thúc 11:01 ném `InvalidTimeException` và giữ nguyên phiên ACTIVE/máy IN_USE; kết thúc 12:00 chuyển COMPLETED/AVAILABLE; kết thúc lại báo lỗi.

```csharp
NetCafe cafe = SampleData.Sample();
Session session = cafe.Sessions[3];
cafe.EndSession(session, new DateTime(2026, 10, 5, 12, 0, 0));
Receipt receipt = cafe.IssueReceipt("R03", session, cafe.Employees[0]);
cafe.Pay("P02", receipt, session.TimeEnd, PaymentMethod.CASH, receipt.TotalAmount);
```

Khi dùng ví dụ trong file khác, thêm `using NetCafeMis;`, `using NetCafeMis.Models;`, `using NetCafeMis.Services;` và `using NetCafeMis.Enums;`.

## Đối chiếu Java → C#

| Java | C# |
|---|---|
| `BigDecimal` | `decimal`; literal tiền/tỷ lệ dùng hậu tố `m` |
| `LocalDateTime` | `DateTime`; mẫu dùng giờ địa phương với Kind Unspecified |
| `timeEnd == null` | `DateTime? TimeEnd` |
| `getAccount().getCustomer()` | `Account.Customer` |
| `getSessions().get(3)` | `Sessions[3]` |
| `endSession(...)` | `EndSession(...)` |
| `RoundingMode.HALF_UP` | `MidpointRounding.AwayFromZero` |
| Getter danh sách chỉ đọc | `ReadOnlyCollection<T>` |

Tiền máy dùng số giây nguyên như Java, tính đơn giá theo giờ rồi làm tròn đến đồng. Các tỷ lệ và tiền giảm được chốt khi lập hóa đơn. C# kiểm tra thêm giá trị enum để từ chối số nguyên ép kiểu không hợp lệ.

`decimal` có khoảng 28–29 chữ số có nghĩa, không có độ chính xác tùy ý như `BigDecimal`. Giá trị vượt phạm vi có thể gây `OverflowException`. Giờ mô phỏng dùng cùng quy ước địa phương; chưa triển khai chuyển múi giờ.

Phạm vi giống bản Java: dữ liệu trong bộ nhớ, chưa có giao diện đồ họa, database, xác thực mật khẩu, nạp/trừ số dư hoặc nâng hạng khách. Hash trong demo chỉ là placeholder. Trạng thái máy vẫn có phương thức cập nhật công khai, nên luồng quản lý phiên nên đi qua các service.

## Kết quả kiểm tra

Build Release với .NET SDK 8.0.425: không lỗi, không cảnh báo. Đã chạy 309 kiểm tra chuyển từ Java và 18 kiểm tra riêng cho C#: tổng cộng **327 kiểm tra đều qua**. Console dữ liệu mẫu khớp `../EXPECTED_OUTPUT.txt`; kết quả ba demo khớp bản Java.

| Bộ kiểm tra | Số kiểm tra |
|---|---:|
| NetCafeChecks | 43 |
| DiscountChecks | 42 |
| AccountChecks | 54 |
| EarlyEndChecks | 40 |
| SessionEndChecks | 130 |
| CSharpCompatibilityChecks | 18 |
| Tổng | 327 |
