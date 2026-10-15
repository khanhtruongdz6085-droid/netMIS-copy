package netcafe.dao;

import netcafe.enums.SessionStatus;
import netcafe.model.Payment;
import netcafe.model.Receipt;
import netcafe.model.Session;
import netcafe.model.customer.Customer;
import netcafe.dao.QueryResults.CustomerSessions;
import netcafe.dao.QueryResults.CustomerAmount;
import java.math.BigDecimal;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.Set;
import static java.util.stream.Collectors.groupingBy;
import static java.util.stream.Collectors.counting;
import static java.util.stream.Collectors.reducing;
import static java.util.stream.Collectors.toSet;

public final class CustomerDAO extends ListDAO<Customer> {
    private final List<Session> sessions;
    private final List<Receipt> receipts;
    private final List<Payment> payments;

    public CustomerDAO(List<Customer> customers, List<Session> sessions,
                       List<Receipt> receipts, List<Payment> payments) {
        super(customers);
        this.sessions = view(sessions);
        this.receipts = view(receipts);
        this.payments = view(payments);
    }

    /** Q12: Khách chưa có phiên trên bất kỳ tài khoản nào. */
    public List<Customer> findNeverPlayed() {
        return result(values.stream().filter(c -> sessions.stream()
            .noneMatch(s -> s.getAccount().getCustomer().getId().equals(c.getId())))
            .sorted(Comparator.comparing(Customer::getId)));
    }

    /** Q13: Đếm phiên COMPLETED của mọi khách, kể cả 0. */
    public List<CustomerSessions> countCompletedSessions() {
        Map<String, Long> counts = sessions.stream().filter(s -> s.getSessionStatus() == SessionStatus.COMPLETED)
            .collect(groupingBy(s -> s.getAccount().getCustomer().getId(), counting()));
        return result(values.stream().map(c -> new CustomerSessions(c, counts.getOrDefault(c.getId(), 0L)))
            .sorted(Comparator.comparingLong(CustomerSessions::getCount).reversed()
                .thenComparing(r -> r.getCustomer().getId())));
    }

    /** Q25: Tối đa ba khách đã thanh toán, tổng giảm dần rồi mã tăng dần. */
    public List<CustomerAmount> topThreePayingCustomers() {
        Map<String, BigDecimal> totals = payments.stream().collect(groupingBy(
            p -> p.getReceipt().getSession().getAccount().getCustomer().getId(),
            reducing(BigDecimal.ZERO, Payment::getAmount, BigDecimal::add)));
        return result(values.stream().filter(c -> totals.containsKey(c.getId()))
            .map(c -> new CustomerAmount(c, totals.get(c.getId())))
            .sorted(Comparator.comparing(CustomerAmount::getAmount).reversed()
                .thenComparing(r -> r.getCustomer().getId())).limit(3));
    }

    /** Q27: Tiền còn phải thu của khách có hóa đơn chưa trả. */
    public List<CustomerAmount> outstandingByCustomer() {
        Set<String> paid = payments.stream().map(p -> p.getReceipt().getId()).collect(toSet());
        Map<String, BigDecimal> totals = receipts.stream().filter(r -> !paid.contains(r.getId()))
            .collect(groupingBy(r -> r.getSession().getAccount().getCustomer().getId(),
                reducing(BigDecimal.ZERO, Receipt::getTotalAmount, BigDecimal::add)));
        return result(values.stream().filter(c -> totals.containsKey(c.getId()))
            .map(c -> new CustomerAmount(c, totals.get(c.getId())))
            .sorted(Comparator.comparing(CustomerAmount::getAmount).reversed()
                .thenComparing(r -> r.getCustomer().getId())));
    }
}
