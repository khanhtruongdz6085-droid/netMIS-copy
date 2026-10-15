package netcafe;

import netcafe.Service.NetCafe;
import netcafe.Service.FoodService;
import netcafe.dao.*;
import netcafe.model.Session;
import netcafe.model.Receipt;
import netcafe.model.NetSpecs.Computer;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.List;
import java.util.function.Function;

/** Prints all 30 DAO queries over the fixture created by Main.sample(). */
public final class QueryDemo {
    private QueryDemo() {}

    private static <T> void show(String title, List<T> rows, Function<T, String> format) {
        System.out.println(title);
        if (rows.isEmpty()) System.out.println("  []");
        rows.forEach(row -> System.out.println("  " + format.apply(row)));
    }

    private static String computer(Computer c) {
        return c.getId() + " section=" + c.getSection().getSectionName()
            + " hourlyPrice=" + c.getSection().getSectionPrice();
    }

    private static String session(Session s) {
        return s.getId() + " customer=" + s.getAccount().getCustomer().getName()
            + " computer=" + s.getComputer().getId() + " section=" + s.getComputer().getSection().getSectionName()
            + " start=" + s.getTimeStart() + " end=" + s.getTimeEnd() + " status=" + s.getSessionStatus();
    }

    private static String receipt(Receipt r) {
        return r.getId() + " session=" + r.getSession().getId()
            + " customer=" + r.getSession().getAccount().getCustomer().getName()
            + " employee=" + r.getEmployee().getEmployeeName() + " total=" + r.getTotalAmount();
    }

    public static void print(NetCafe cafe) {
        ComputerDAO computers = cafe.getComputerDAO();
        ComputerSectionDAO sections = cafe.getComputerSectionDAO();
        SessionDAO sessions = cafe.getSessionDAO();
        CustomerDAO customers = cafe.getCustomerDAO();
        FoodDAO foods = cafe.getFoodDAO();
        FoodServiceDAO orders = cafe.getFoodServiceDAO();
        EmployeeDAO employees = cafe.getEmployeeDAO();
        ReceiptDAO receipts = cafe.getReceiptDAO();
        PaymentDAO payments = cafe.getPaymentDAO();
        LocalDate day = LocalDate.of(2026, 10, 5);

        show("Q01 May dang trong", computers.findAvailable(), QueryDemo::computer);
        show("Q02 May bao tri", computers.findMaintenance(), QueryDemo::computer);
        show("Q03 So may theo khu va trang thai", sections.countComputersByStatus(),
            r -> r.getSection().getId() + " " + r.getStatus() + " count=" + r.getCount());
        show("Q04 Khu gia gio > 15000", sections.findAboveHourlyPrice(new BigDecimal("15000")),
            s -> s.getId() + " " + s.getSectionName() + " hourlyPrice=" + s.getSectionPrice());
        show("Q05 Lich su may PC01", sessions.historyByComputer("PC01"), QueryDemo::session);
        show("Q06 Phien dang hoat dong", sessions.findActive(), QueryDemo::session);
        show("Q07 Phien hoan tat >= 120 phut", sessions.findCompletedAtLeastMinutes(120),
            s -> session(s) + " minutes=" + SessionDAO.completedMinutes(s));
        show("Q08 Phut choi theo may, ngay 2026-10-05", sessions.completedMinutesByComputerOn(day),
            r -> r.getComputer().getId() + " minutes=" + r.getMinutes());
        show("Q09 May chua tung su dung", computers.findNeverUsed(), QueryDemo::computer);
        show("Q10 Tam tinh phien ACTIVE tai 12:00", sessions.estimateActiveAmounts(day.atTime(12, 0)),
            r -> r.getSession().getId() + " amount=" + r.getAmount());

        show("Q11 Lich su khach C01", sessions.historyByCustomer("C01"), QueryDemo::session);
        show("Q12 Khach chua tung choi", customers.findNeverPlayed(), c -> c.getId() + " " + c.getName());
        show("Q13 So phien hoan tat theo khach", customers.countCompletedSessions(),
            r -> r.getCustomer().getId() + " count=" + r.getCount());
        show("Q14 Mon gia <= 15000", foods.findAtMostPrice(new BigDecimal("15000")),
            f -> f.getId() + " " + f.getFoodName() + " price=" + f.getPrice());
        show("Q15 Chi tiet goi mon phien S01", orders.orderDetailsBySession("S01"),
            d -> d.getService().getId() + " time=" + d.getService().getTimeStartService()
                + " employee=" + d.getService().getEmployee().getEmployeeName() + " food=" + d.getFood().getFoodName()
                + " quantity=" + d.getOrderQuantity() + " price=" + d.getPriceAtOrder() + " amount=" + d.calculateAmount());
        show("Q16 Tong tien mon goc tung phien", orders.totalsBySession(),
            r -> r.getSession().getId() + " amount=" + r.getAmount());
        show("Q17 Phien hoan tat khong goi mon", sessions.findCompletedWithoutFood(), QueryDemo::session);
        show("Q18 Mon duoc goi nhieu nhat", foods.findMostOrdered(),
            r -> r.getFood().getId() + " " + r.getFood().getFoodName() + " quantity=" + r.getQuantity());
        show("Q19 Lan goi mon > 20000", orders.findAboveTotal(new BigDecimal("20000")),
            s -> s.getId() + " customer=" + s.getSession().getAccount().getCustomer().getName() + " total=" + s.getTotal());
        show("Q20 Goi mon theo nhan vien, ngay 2026-10-05", employees.foodStatsOn(day),
            r -> r.getEmployee().getId() + " count=" + r.getCount() + " amount=" + r.getAmount());

        show("Q21 Hoa don chua thanh toan", receipts.findUnpaid(), QueryDemo::receipt);
        show("Q22 Thuc thu theo ngay", payments.revenueByDay(), r -> r.getDay() + " amount=" + r.getAmount());
        show("Q23 Thuc thu theo phuong thuc", payments.revenueByMethod(),
            r -> r.getMethod() + " count=" + r.getCount() + " amount=" + r.getAmount());
        show("Q24 Hoa don lon nhat", receipts.findLargest(), QueryDemo::receipt);
        show("Q25 Ba khach tra nhieu nhat", customers.topThreePayingCustomers(),
            r -> r.getCustomer().getId() + " " + r.getCustomer().getName() + " amount=" + r.getAmount());
        show("Q26 Hoa don theo nhan vien lap", employees.billingStats(),
            r -> r.getEmployee().getId() + " count=" + r.getCount() + " total=" + r.getTotal()
                + " collected=" + r.getCollected() + " outstanding=" + r.getOutstanding());
        show("Q27 Tien con phai thu theo khach", customers.outstandingByCustomer(),
            r -> r.getCustomer().getId() + " amount=" + r.getAmount());
        show("Q28 Thuc thu theo khu", payments.revenueBySection(),
            r -> r.getSection().getId() + " computer=" + r.getComputerAmount()
                + " food=" + r.getFoodAmount() + " collected=" + r.getCollected());
        show("Q29 Thanh toan sai lech", receipts.findPaymentIssues(),
            r -> r.getReceipt().getId() + " count=" + r.getCount() + " paid=" + r.getPaidAmount()
                + " reasons=" + r.getReasons());
        show("Q30 Phien hoan tat chua co hoa don", sessions.findCompletedWithoutReceipt(), QueryDemo::session);
    }
}
