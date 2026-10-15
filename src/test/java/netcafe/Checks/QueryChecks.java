package netcafe.Checks;

import netcafe.Main;
import netcafe.Identifiable;
import netcafe.Service.*;
import netcafe.dao.*;
import netcafe.dao.QueryResults.*;
import netcafe.enums.*;
import netcafe.exception.*;
import netcafe.model.*;
import netcafe.model.NetSpecs.*;
import netcafe.model.customer.*;
import java.math.BigDecimal;
import java.time.*;
import java.util.*;
import java.util.stream.*;

/** Check every specified query, shared data, day boundaries, ties and invalid imports. */
public final class QueryChecks {
    private static int checks;
    private static final LocalDate DAY = LocalDate.of(2026, 10, 5);
    private static BigDecimal money(long value) { return BigDecimal.valueOf(value); }
    private static LocalDateTime at(int hour, int minute) { return DAY.atTime(hour, minute); }

    private static void equal(Object expected, Object actual) {
        checks++;
        boolean same = expected instanceof Number && actual instanceof Number
            ? new BigDecimal(expected.toString()).compareTo(new BigDecimal(actual.toString())) == 0
            : Objects.equals(expected, actual);
        if (!same) throw new AssertionError("Expected " + expected + ", got " + actual);
    }

    private static void fails(Class<? extends RuntimeException> type, Runnable action) {
        checks++;
        try { action.run(); }
        catch (RuntimeException failure) {
            if (type.isInstance(failure)) return;
            throw new AssertionError("Expected " + type.getSimpleName(), failure);
        }
        throw new AssertionError("Expected " + type.getSimpleName());
    }

    private static String ids(List<? extends Identifiable> rows) {
        return rows.stream().map(Identifiable::getId).collect(Collectors.joining(","));
    }

    private static void sampleQueries() {
        NetCafe c = Main.sample();
        equal("PC01,PC03,PC04,PC06", ids(c.getComputerDAO().findAvailable())); // Q01
        equal("PC05", ids(c.getComputerDAO().findMaintenance())); // Q02
        List<ComputerCount> counts = c.getComputerSectionDAO().countComputersByStatus(); // Q03
        equal(6, counts.size());
        equal(1, counts.stream().filter(r -> r.getSection().getId().equals("CS01") && r.getStatus() == ComputerStatus.AVAILABLE).findFirst().get().getCount());
        equal(3, counts.stream().filter(r -> r.getSection().getId().equals("CS02") && r.getStatus() == ComputerStatus.AVAILABLE).findFirst().get().getCount());
        equal(0, counts.stream().filter(r -> r.getSection().getId().equals("CS02") && r.getStatus() == ComputerStatus.IN_USE).findFirst().get().getCount());
        equal("CS02", ids(c.getComputerSectionDAO().findAboveHourlyPrice(money(15000)))); // Q04
        equal(0, c.getComputerSectionDAO().findAboveHourlyPrice(money(20000)).size());
        equal("S01", ids(c.getSessionDAO().historyByComputer("PC01"))); // Q05
        equal("S04", ids(c.getSessionDAO().findActive())); // Q06
        equal("S01", ids(c.getSessionDAO().findCompletedAtLeastMinutes(120))); // Q07
        equal(120, SessionDAO.completedMinutes(c.getSessions().get(0)));
        List<ComputerMinutes> minutes = c.getSessionDAO().completedMinutesByComputerOn(DAY); // Q08
        equal(6, minutes.size());
        equal("120,0,90,60,0,0", minutes.stream().map(r -> String.valueOf(r.getMinutes())).collect(Collectors.joining(",")));
        equal("PC05,PC06", ids(c.getComputerDAO().findNeverUsed())); // Q09
        List<SessionCharge> estimate = c.getSessionDAO().estimateActiveAmounts(at(12, 0)); // Q10
        equal(1, estimate.size()); equal(12000, estimate.get(0).getAmount());
        equal(null, estimate.get(0).getSession().getTimeEnd());
        equal(SessionStatus.ACTIVE, estimate.get(0).getSession().getSessionStatus());
        fails(InvalidTimeException.class, () -> c.getSessionDAO().estimateActiveAmounts(at(10, 59)));
        equal("S04,S01", ids(c.getSessionDAO().historyByCustomer(" C01 "))); // Q11
        equal("C04", ids(c.getCustomerDAO().findNeverPlayed())); // Q12
        List<CustomerSessions> sessions = c.getCustomerDAO().countCompletedSessions(); // Q13
        equal(4, sessions.size());
        equal("C01:1,C02:1,C03:1,C04:0", sessions.stream().map(r -> r.getCustomer().getId() + ":" + r.getCount()).collect(Collectors.joining(",")));
        equal("F03,F01,F02", ids(c.getFoodDAO().findAtMostPrice(money(15000)))); // Q14
        List<FoodOrderDetail> details = c.getFoodServiceDAO().orderDetailsBySession("S01"); // Q15
        equal(2, details.size()); equal("SV01", details.get(0).getService().getId());
        equal("E01", details.get(0).getService().getEmployee().getId());
        equal("F01", details.get(0).getFood().getId()); equal(1, details.get(0).getOrderQuantity());
        equal(15000, details.get(0).getPriceAtOrder()); equal(15000, details.get(0).calculateAmount());
        equal(10000, details.get(1).calculateAmount());
        List<SessionFoodTotal> foodTotals = c.getFoodServiceDAO().totalsBySession(); // Q16
        equal("S02:30000,S01:25000,S04:20000,S03:0", foodTotals.stream().map(r -> r.getSession().getId() + ":" + r.getAmount()).collect(Collectors.joining(",")));
        equal("S03", ids(c.getSessionDAO().findCompletedWithoutFood())); // Q17
        List<FoodQuantity> best = c.getFoodDAO().findMostOrdered(); // Q18: both tied foods must remain.
        equal(2, best.size()); equal("F01", best.get(0).getFood().getId()); equal(3, best.get(0).getQuantity());
        equal("F02", best.get(1).getFood().getId()); equal(3, best.get(1).getQuantity());
        equal("SV02,SV01", ids(c.getFoodServiceDAO().findAboveTotal(money(20000)))); // Q19
        List<EmployeeFoodStats> foodStats = c.getEmployeeDAO().foodStatsOn(DAY); // Q20
        equal(2, foodStats.get(0).getCount()); equal(45000, foodStats.get(0).getAmount());
        equal(1, foodStats.get(1).getCount()); equal(30000, foodStats.get(1).getAmount());
        equal(0, foodStats.get(2).getCount()); equal(0, foodStats.get(2).getAmount());
        equal("R02", ids(c.getReceiptDAO().findUnpaid())); // Q21
        equal(1, c.getPaymentDAO().revenueByDay().size()); // Q22
        equal(DAY, c.getPaymentDAO().revenueByDay().get(0).getDay());
        equal(49000, c.getPaymentDAO().revenueByDay().get(0).getAmount());
        List<MethodRevenue> methods = c.getPaymentDAO().revenueByMethod(); // Q23
        equal(3, methods.size());
        equal(2, methods.stream().filter(r -> r.getCount() == 0 && r.getAmount().signum() == 0).count());
        equal(49000, methods.stream().filter(r -> r.getMethod() == PaymentMethod.CASH).findFirst().get().getAmount());
        equal("R02", ids(c.getReceiptDAO().findLargest())); // Q24
        equal(1, c.getCustomerDAO().topThreePayingCustomers().size()); // Q25
        equal("C01", c.getCustomerDAO().topThreePayingCustomers().get(0).getCustomer().getId());
        equal(49000, c.getCustomerDAO().topThreePayingCustomers().get(0).getAmount());
        List<EmployeeBilling> billing = c.getEmployeeDAO().billingStats(); // Q26
        equal(1, billing.get(0).getCount()); equal(60000, billing.get(0).getTotal());
        equal(0, billing.get(0).getCollected()); equal(60000, billing.get(0).getOutstanding());
        equal(49000, billing.get(1).getCollected()); equal(0, billing.get(1).getOutstanding());
        equal(0, billing.get(2).getCount()); equal(0, billing.get(2).getTotal());
        equal(0, billing.get(2).getCollected()); equal(0, billing.get(2).getOutstanding());
        List<CustomerAmount> outstanding = c.getCustomerDAO().outstandingByCustomer(); // Q27
        equal(1, outstanding.size()); equal("C02", outstanding.get(0).getCustomer().getId()); equal(60000, outstanding.get(0).getAmount());
        List<SectionRevenue> revenue = c.getPaymentDAO().revenueBySection(); // Q28
        equal(2, revenue.size()); equal(24000, revenue.get(0).getComputerAmount()); equal(25000, revenue.get(0).getFoodAmount());
        equal(49000, revenue.get(0).getCollected()); equal(0, revenue.get(1).getCollected());
        equal(0, c.getReceiptDAO().findPaymentIssues().size()); // Q29
        equal("S03", ids(c.getSessionDAO().findCompletedWithoutReceipt())); // Q30
    }

    private static void emptyAndValidation() {
        NetCafe c = new NetCafe();
        equal(0, c.getComputerDAO().findAvailable().size());
        equal(0, c.getComputerDAO().findMaintenance().size());
        equal(0, c.getComputerSectionDAO().countComputersByStatus().size());
        equal(0, c.getComputerSectionDAO().findAboveHourlyPrice(money(0)).size());
        equal(0, c.getSessionDAO().historyByComputer("missing").size());
        equal(0, c.getSessionDAO().findActive().size());
        equal(0, c.getSessionDAO().findCompletedAtLeastMinutes(120).size());
        equal(0, c.getSessionDAO().completedMinutesByComputerOn(DAY).size());
        equal(0, c.getComputerDAO().findNeverUsed().size());
        equal(0, c.getSessionDAO().estimateActiveAmounts(at(12, 0)).size());
        equal(0, c.getSessionDAO().historyByCustomer("missing").size());
        equal(0, c.getCustomerDAO().findNeverPlayed().size());
        equal(0, c.getCustomerDAO().countCompletedSessions().size());
        equal(0, c.getFoodDAO().findAtMostPrice(money(0)).size());
        equal(0, c.getFoodServiceDAO().orderDetailsBySession("missing").size());
        equal(0, c.getFoodServiceDAO().totalsBySession().size());
        equal(0, c.getSessionDAO().findCompletedWithoutFood().size());
        equal(0, c.getFoodDAO().findMostOrdered().size());
        equal(0, c.getFoodServiceDAO().findAboveTotal(money(0)).size());
        equal(0, c.getEmployeeDAO().foodStatsOn(DAY).size());
        equal(0, c.getReceiptDAO().findUnpaid().size());
        equal(0, c.getPaymentDAO().revenueByDay().size());
        equal(3, c.getPaymentDAO().revenueByMethod().size());
        equal(0, c.getPaymentDAO().revenueByMethod().stream().mapToLong(MethodRevenue::getCount).sum());
        equal(0, c.getReceiptDAO().findLargest().size());
        equal(0, c.getCustomerDAO().topThreePayingCustomers().size());
        equal(0, c.getEmployeeDAO().billingStats().size());
        equal(0, c.getCustomerDAO().outstandingByCustomer().size());
        equal(0, c.getPaymentDAO().revenueBySection().size());
        equal(0, c.getReceiptDAO().findPaymentIssues().size());
        equal(0, c.getSessionDAO().findCompletedWithoutReceipt().size());
        equal(0, c.getPaymentDAO().collectedAmount());
        fails(ValidationException.class, () -> c.getComputerDAO().findById(null));
        fails(ValidationException.class, () -> c.getCustomerAccountDAO().findByUsername(" "));
        fails(ValidationException.class, () -> c.getSessionDAO().completedMinutesByComputerOn(null));
        fails(ValidationException.class, () -> c.getEmployeeDAO().foodStatsOn(null));
        fails(ValidationException.class, () -> c.getSessionDAO().findCompletedAtLeastMinutes(-1));
        fails(ValidationException.class, () -> c.getFoodDAO().findAtMostPrice(null));
        fails(ValidationException.class, () -> c.getComputerSectionDAO().findAboveHourlyPrice(money(-1)));
        fails(InvalidTimeException.class, () -> c.getSessionDAO().estimateActiveAmounts(null));
    }

    private static void sharedListsAndMoney() {
        NetCafe c = Main.sample();
        ComputerDAO dao = c.getComputerDAO();
        List<Computer> live = dao.findAll();
        List<Computer> snapshot = dao.findAvailable();
        equal(true, dao.findById(" PC01 ").get() == c.getComputers().get(0));
        equal(true, c.getComputerSectionDAO().findById("CS01").get() == dao.findById("PC01").get().getSection());
        equal(true, c.getCustomerAccountDAO().findByUsername(" YAMAL ").get() == c.getAccounts().get(0));
        equal(true, c.getCustomerDAO().findById("C01").get() == c.getCustomerAccountDAO().findById("A01").get().getCustomer());
        fails(UnsupportedOperationException.class, () -> live.clear());
        fails(UnsupportedOperationException.class, () -> snapshot.clear());
        ComputerSection empty = new ComputerSection("CS03", "Empty", money(15000));
        c.addSection(empty);
        equal(3, c.getComputerSectionDAO().countComputersByStatus().stream().filter(r -> r.getSection() == empty && r.getCount() == 0).count());
        equal(0, c.getPaymentDAO().revenueBySection().get(2).getCollected());
        c.addComputer(new Computer("PC07", empty, ComputerStatus.AVAILABLE));
        equal(7, live.size()); equal(4, snapshot.size()); equal(5, dao.findAvailable().size());
        c.endSession(c.getSessionDAO().findById("S04").get(), at(12, 0));
        equal(0, c.getSessionDAO().findActive().size());
        equal(6, dao.findAvailable().size());
        Receipt receipt = c.issueReceipt("R03", c.getSessions().get(3), c.getEmployees().get(0));
        equal("R02,R03", ids(c.getReceiptDAO().findUnpaid()));
        c.pay("P02", receipt, at(12, 0), PaymentMethod.CARD, receipt.getTotalAmount());
        equal("R02", ids(c.getReceiptDAO().findUnpaid())); equal(81000, c.getPaymentDAO().collectedAmount());
        c.getAccounts().get(0).setBalance(new BigDecimal("100.50"));
        equal(new BigDecimal("100.50"), c.getCustomerAccountDAO().findById("A01").get().getBalance());
        c.getFoods().get(0).setPrice(money(99999));
        equal(15000, c.getFoodServiceDAO().orderDetailsBySession("S01").get(0).getPriceAtOrder());
        equal(30000, c.getFoodServiceDAO().totalsBySession().get(0).getAmount());
    }

    private static void dayBoundaries() {
        NetCafe c = Main.sample();
        Employee employee = c.getEmployees().get(0);
        Food food = c.getFoods().get(2);
        CustomerAccount account = c.getAccounts().get(3);
        Computer computer = c.getComputers().get(5);
        LocalDateTime midnight = DAY.atStartOfDay();
        Session previous = c.openSession("S05", account, computer, midnight.minusMinutes(30));
        c.order("SV04", previous, employee, midnight.minusMinutes(1), new Food[]{food}, new int[]{1});
        c.order("SV05", previous, employee, midnight, new Food[]{food}, new int[]{1});
        c.endSession(previous, midnight.plusMinutes(30));
        Session current = c.openSession("S06", account, computer, DAY.atTime(23, 0));
        c.order("SV06", current, employee, DAY.atTime(23, 59), new Food[]{food}, new int[]{1});
        c.endSession(current, midnight.plusDays(1));
        Session next = c.openSession("S07", account, computer, midnight.plusDays(1));
        c.order("SV07", next, employee, midnight.plusDays(1), new Food[]{food}, new int[]{1});
        c.endSession(next, midnight.plusDays(1).plusHours(1));
        equal(60, c.getSessionDAO().completedMinutesByComputerOn(DAY).get(5).getMinutes());
        equal(60, c.getSessionDAO().completedMinutesByComputerOn(DAY.minusDays(1)).get(5).getMinutes());
        equal(60, c.getSessionDAO().completedMinutesByComputerOn(DAY.plusDays(1)).get(5).getMinutes());
        equal(4, c.getEmployeeDAO().foodStatsOn(DAY).get(0).getCount());
        equal(59000, c.getEmployeeDAO().foodStatsOn(DAY).get(0).getAmount());
        equal(1, c.getEmployeeDAO().foodStatsOn(DAY.plusDays(1)).get(0).getCount());
        Receipt receipt = c.issueReceipt("R03", c.getSessions().get(2), employee);
        c.pay("P02", receipt, midnight.plusDays(1), PaymentMethod.CARD, receipt.getTotalAmount());
        equal(2, c.getPaymentDAO().revenueByDay().size());
        equal(49000, c.getPaymentDAO().revenueByDay().get(0).getAmount());
        equal(DAY.plusDays(1), c.getPaymentDAO().revenueByDay().get(1).getDay());
        equal(20000, c.getPaymentDAO().revenueByDay().get(1).getAmount());
    }

    private static void tiesAndAccounts() {
        NetCafe c = Main.sample();
        Employee employee = c.getEmployees().get(0);
        Session session = c.openSession("S05", c.getAccounts().get(3), c.getComputers().get(5), at(12, 0));
        c.endSession(session, at(15, 0));
        Receipt fourth = c.issueReceipt("R04", session, employee);
        equal("R02,R04", ids(c.getReceiptDAO().findLargest()));
        Receipt third = c.issueReceipt("R03", c.getSessions().get(2), employee);
        c.pay("P02", c.getReceipts().get(1), at(12, 0), PaymentMethod.BANK_TRANSFER, money(60000));
        c.pay("P03", third, at(12, 0), PaymentMethod.CARD, third.getTotalAmount());
        c.pay("P04", fourth, at(15, 0), PaymentMethod.CASH, fourth.getTotalAmount());
        equal("C02,C04,C01", c.getCustomerDAO().topThreePayingCustomers().stream().map(r -> r.getCustomer().getId()).collect(Collectors.joining(",")));
        CustomerAccount second = new CustomerAccount("A05", "second", "test-hash", money(0), AccountStatus.ACTIVE, c.getCustomers().get(0));
        c.addAccount(second);
        Session extra = c.openSession("S06", second, c.getComputers().get(0), at(16, 0));
        c.endSession(extra, at(17, 0));
        Receipt fifth = c.issueReceipt("R05", extra, employee);
        c.pay("P05", fifth, at(17, 0), PaymentMethod.CARD, fifth.getTotalAmount());
        equal("C01,C02,C04", c.getCustomerDAO().topThreePayingCustomers().stream().map(r -> r.getCustomer().getId()).collect(Collectors.joining(",")));
        equal(61000, c.getCustomerDAO().topThreePayingCustomers().get(0).getAmount());
        equal("S06,S04,S01", ids(c.getSessionDAO().historyByCustomer("C01")));
        equal(2, c.getCustomerDAO().countCompletedSessions().get(0).getCount());
        equal(0, c.getCustomerDAO().outstandingByCustomer().size());

        NetCafe free = Main.sample();
        ComputerSection zero = new ComputerSection("CS00", "Free", money(0));
        Computer pc = new Computer("PC00", zero, ComputerStatus.AVAILABLE);
        free.addSection(zero); free.addComputer(pc);
        Session freeSession = free.openSession("FREE", free.getAccounts().get(3), pc, at(12, 0));
        free.endSession(freeSession, at(13, 0));
        Receipt freeReceipt = free.issueReceipt("R00", freeSession, free.getEmployees().get(0));
        equal(2, free.getCustomerDAO().outstandingByCustomer().size()); // Includes a zero-valued unpaid receipt.
        free.pay("P00", freeReceipt, at(13, 0), PaymentMethod.CASH, money(0));
        equal(2, free.getCustomerDAO().topThreePayingCustomers().size()); // Includes a customer with a valid zero payment.
        equal("C04", free.getCustomerDAO().topThreePayingCustomers().get(1).getCustomer().getId());
        equal(0, free.getCustomerDAO().topThreePayingCustomers().get(1).getAmount());
    }

    private static void auditImportedPayments() {
        NetCafe c = Main.sample();
        Receipt canonical = c.getReceipts().get(0);
        Payment duplicate = new Payment("PDUP", canonical, at(12, 0), PaymentMethod.CARD, canonical.getTotalAmount());
        ReceiptDAO duplicateDAO = new ReceiptDAO(c.getReceipts(), Arrays.asList(c.getPayments().get(0), duplicate));
        equal(Arrays.asList("MULTIPLE_PAYMENTS"), duplicateDAO.findPaymentIssues().get(0).getReasons());
        equal(98000, duplicateDAO.findPaymentIssues().get(0).getPaidAmount());
        // Simulate an imported payment referring to another receipt object with the same ID but stale total.
        Receipt stale = new Receipt(canonical.getId(), canonical.getSession(), canonical.getEmployee(), money(0), money(1));
        Payment wrong = new Payment("PBAD", stale, at(12, 0), PaymentMethod.CARD, money(1));
        ReceiptDAO mismatchDAO = new ReceiptDAO(c.getReceipts(), Collections.singletonList(wrong));
        equal(1, mismatchDAO.findPaymentIssues().size());
        equal(Arrays.asList("AMOUNT_MISMATCH"), mismatchDAO.findPaymentIssues().get(0).getReasons());
        ReceiptDAO both = new ReceiptDAO(c.getReceipts(), Arrays.asList(c.getPayments().get(0), wrong));
        PaymentIssue issue = both.findPaymentIssues().get(0);
        equal("R01", issue.getReceipt().getId()); equal(2, issue.getCount());
        equal(Arrays.asList("MULTIPLE_PAYMENTS", "AMOUNT_MISMATCH"), issue.getReasons());
        fails(UnsupportedOperationException.class, () -> issue.getReasons().clear());
        PaymentDAO malformed = new PaymentDAO(Arrays.asList(c.getPayments().get(0), duplicate), c.getSections());
        equal(98000, malformed.revenueBySection().get(0).getCollected());
        equal(24000, malformed.revenueBySection().get(0).getComputerAmount());
        equal(25000, malformed.revenueBySection().get(0).getFoodAmount());
    }

    public static void main(String[] args) {
        sampleQueries();
        emptyAndValidation();
        sharedListsAndMoney();
        dayBoundaries();
        tiesAndAccounts();
        auditImportedPayments();
        System.out.println("QueryChecks OK: " + checks + " checks");
    }
}
