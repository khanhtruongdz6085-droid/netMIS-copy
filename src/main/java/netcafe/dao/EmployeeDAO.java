package netcafe.dao;

import netcafe.Service.FoodService;
import netcafe.model.Checks;
import netcafe.model.Payment;
import netcafe.model.Receipt;
import netcafe.model.NetSpecs.Employee;
import netcafe.dao.QueryResults.EmployeeFoodStats;
import netcafe.dao.QueryResults.EmployeeBilling;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.Comparator;
import java.util.List;
import java.util.Set;
import static java.util.stream.Collectors.toList;
import static java.util.stream.Collectors.toSet;

public final class EmployeeDAO extends ListDAO<Employee> {
    private final List<FoodService> services;
    private final List<Receipt> receipts;
    private final List<Payment> payments;

    public EmployeeDAO(List<Employee> employees, List<FoodService> services,
                       List<Receipt> receipts, List<Payment> payments) {
        super(employees);
        this.services = view(services);
        this.receipts = view(receipts);
        this.payments = view(payments);
    }

    /** Q20: Lần gọi và tổng tiền gốc theo nhân viên/ngày gọi, kể cả 0. */
    public List<EmployeeFoodStats> foodStatsOn(LocalDate day) {
        LocalDateTime start = Checks.required(day, "day").atStartOfDay();
        LocalDateTime end = start.plusDays(1);
        List<FoodService> daily = services.stream().filter(s -> !s.getTimeStartService().isBefore(start)
            && s.getTimeStartService().isBefore(end)).collect(toList());
        return result(values.stream().sorted(Comparator.comparing(Employee::getId)).map(employee -> {
            List<FoodService> orders = daily.stream().filter(s -> s.getEmployee().getId().equals(employee.getId())).collect(toList());
            return new EmployeeFoodStats(employee, orders.size(), orders.stream().map(FoodService::getTotal)
                .reduce(BigDecimal.ZERO, BigDecimal::add));
        }));
    }

    /** Q26: Hóa đơn do từng nhân viên lập; tổng/đã thu/chưa thu, kể cả 0. */
    public List<EmployeeBilling> billingStats() {
        Set<String> paid = payments.stream().map(p -> p.getReceipt().getId()).collect(toSet());
        return result(values.stream().sorted(Comparator.comparing(Employee::getId)).map(employee -> {
            List<Receipt> issued = receipts.stream().filter(r -> r.getEmployee().getId().equals(employee.getId())).collect(toList());
            Set<String> ids = issued.stream().map(Receipt::getId).collect(toSet());
            BigDecimal total = issued.stream().map(Receipt::getTotalAmount).reduce(BigDecimal.ZERO, BigDecimal::add);
            BigDecimal collected = payments.stream().filter(p -> ids.contains(p.getReceipt().getId()))
                .map(Payment::getAmount).reduce(BigDecimal.ZERO, BigDecimal::add);
            BigDecimal outstanding = issued.stream().filter(r -> !paid.contains(r.getId()))
                .map(Receipt::getTotalAmount).reduce(BigDecimal.ZERO, BigDecimal::add);
            return new EmployeeBilling(employee, issued.size(), total, collected, outstanding);
        }));
    }
}
