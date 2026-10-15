package netcafe.dao;

import netcafe.enums.PaymentMethod;
import netcafe.model.Payment;
import netcafe.model.Receipt;
import netcafe.model.NetSpecs.ComputerSection;
import netcafe.dao.QueryResults.DailyRevenue;
import netcafe.dao.QueryResults.MethodRevenue;
import netcafe.dao.QueryResults.SectionRevenue;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.Arrays;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;
import static java.util.stream.Collectors.groupingBy;
import static java.util.stream.Collectors.reducing;
import static java.util.stream.Collectors.toList;
import static java.util.stream.Collectors.toMap;

public final class PaymentDAO extends ListDAO<Payment> {
    private final List<ComputerSection> sections;

    public PaymentDAO(List<Payment> payments, List<ComputerSection> sections) {
        super(payments);
        this.sections = view(sections);
    }

    public BigDecimal collectedAmount() {
        return values.stream().map(Payment::getAmount).reduce(BigDecimal.ZERO, BigDecimal::add);
    }

    /** Q22: Tổng thực thu theo ngày thanh toán, ngày tăng dần. */
    public List<DailyRevenue> revenueByDay() {
        Map<LocalDate, BigDecimal> totals = values.stream().collect(groupingBy(p -> p.getTime().toLocalDate(),
            TreeMap::new, reducing(BigDecimal.ZERO, Payment::getAmount, BigDecimal::add)));
        return result(totals.entrySet().stream().map(e -> new DailyRevenue(e.getKey(), e.getValue())));
    }

    /** Q23: Mọi phương thức, kể cả chưa sử dụng. */
    public List<MethodRevenue> revenueByMethod() {
        return result(Arrays.stream(PaymentMethod.values()).sorted(Comparator.comparing(Enum::name)).map(method -> {
            List<Payment> matching = values.stream().filter(p -> p.getPaymentMethod() == method).collect(toList());
            return new MethodRevenue(method, matching.size(), matching.stream().map(Payment::getAmount)
                .reduce(BigDecimal.ZERO, BigDecimal::add));
        }));
    }

    /** Q28: Thực thu theo khu, tách khoản tiền sau giảm của hóa đơn đã trả. */
    public List<SectionRevenue> revenueBySection() {
        // Each paid receipt contributes its components once, even for malformed imported payments.
        Map<String, Receipt> paidReceipts = values.stream().map(Payment::getReceipt)
            .collect(toMap(Receipt::getId, r -> r, (first, next) -> first));
        return result(sections.stream().sorted(Comparator.comparing(ComputerSection::getId)).map(section -> {
            List<Receipt> matching = paidReceipts.values().stream()
                .filter(r -> r.getSession().getComputer().getSection().getId().equals(section.getId())).collect(toList());
            BigDecimal computer = matching.stream().map(Receipt::getComputerAmount).reduce(BigDecimal.ZERO, BigDecimal::add);
            BigDecimal food = matching.stream().map(Receipt::getServiceAmount).reduce(BigDecimal.ZERO, BigDecimal::add);
            BigDecimal collected = values.stream()
                .filter(p -> p.getReceipt().getSession().getComputer().getSection().getId().equals(section.getId()))
                .map(Payment::getAmount).reduce(BigDecimal.ZERO, BigDecimal::add);
            return new SectionRevenue(section, computer, food, collected);
        }));
    }
}
