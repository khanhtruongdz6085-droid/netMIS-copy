package netcafe.dao;

import netcafe.model.Payment;
import netcafe.model.Receipt;
import netcafe.dao.QueryResults.PaymentIssue;
import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.Set;
import static java.util.stream.Collectors.groupingBy;
import static java.util.stream.Collectors.toSet;

public final class ReceiptDAO extends ListDAO<Receipt> {
    private final List<Payment> payments;

    public ReceiptDAO(List<Receipt> receipts, List<Payment> payments) {
        super(receipts);
        this.payments = view(payments);
    }

    /** Q21: Hóa đơn chưa có Payment, khách và nhân viên qua tham chiếu. */
    public List<Receipt> findUnpaid() {
        Set<String> paid = payments.stream().map(p -> p.getReceipt().getId()).collect(toSet());
        return result(values.stream().filter(r -> !paid.contains(r.getId())).sorted(Comparator.comparing(Receipt::getId)));
    }

    /** Q24: Mọi hóa đơn có tổng tiền lớn nhất; tập rỗng trả danh sách rỗng. */
    public List<Receipt> findLargest() {
        BigDecimal maximum = values.stream().map(Receipt::getTotalAmount).max(BigDecimal::compareTo).orElse(BigDecimal.ZERO);
        return result(values.stream().filter(r -> r.getTotalAmount().compareTo(maximum) == 0)
            .sorted(Comparator.comparing(Receipt::getId)));
    }

    /** Q29: Nhiều Payment hoặc từng khoản Payment khác tổng hóa đơn. */
    public List<PaymentIssue> findPaymentIssues() {
        Map<String, List<Payment>> byReceipt = payments.stream().collect(groupingBy(p -> p.getReceipt().getId()));
        return result(values.stream().sorted(Comparator.comparing(Receipt::getId)).map(receipt -> {
            List<Payment> matching = byReceipt.getOrDefault(receipt.getId(), Collections.emptyList());
            List<String> reasons = new ArrayList<>();
            if (matching.size() > 1) reasons.add("MULTIPLE_PAYMENTS");
            if (matching.stream().anyMatch(p -> p.getAmount().compareTo(receipt.getTotalAmount()) != 0))
                reasons.add("AMOUNT_MISMATCH");
            BigDecimal paid = matching.stream().map(Payment::getAmount).reduce(BigDecimal.ZERO, BigDecimal::add);
            return new PaymentIssue(receipt, matching.size(), paid, reasons);
        }).filter(issue -> !issue.getReasons().isEmpty()));
    }
}
