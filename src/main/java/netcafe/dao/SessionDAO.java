package netcafe.dao;

import netcafe.Service.FoodService;
import netcafe.enums.SessionStatus;
import netcafe.exception.InvalidTimeException;
import netcafe.exception.ValidationException;
import netcafe.model.Checks;
import netcafe.model.Receipt;
import netcafe.model.Session;
import netcafe.model.NetSpecs.Computer;
import netcafe.dao.QueryResults.ComputerMinutes;
import netcafe.dao.QueryResults.SessionCharge;
import java.time.Duration;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import static java.util.stream.Collectors.groupingBy;
import static java.util.stream.Collectors.summingLong;

public final class SessionDAO extends ListDAO<Session> {
    private final List<Computer> computers;
    private final List<FoodService> services;
    private final List<Receipt> receipts;

    public SessionDAO(List<Session> sessions, List<Computer> computers,
                      List<FoodService> services, List<Receipt> receipts) {
        super(sessions);
        this.computers = view(computers);
        this.services = view(services);
        this.receipts = view(receipts);
    }

    private static Comparator<Session> newestFirst() {
        return Comparator.comparing(Session::getTimeStart).reversed()
            .thenComparing(Session::getId, Comparator.reverseOrder());
    }

    /** Q05: Lịch sử máy; lấy tên khách qua account.customer. */
    public List<Session> historyByComputer(String computerId) {
        String key = Checks.id(computerId);
        return result(values.stream().filter(s -> s.getComputer().getId().equals(key)).sorted(newestFirst()));
    }

    /** Q06: Phiên ACTIVE, khách/máy/khu được giữ bằng tham chiếu. */
    public List<Session> findActive() {
        return result(values.stream().filter(s -> s.getSessionStatus() == SessionStatus.ACTIVE)
            .sorted(Comparator.comparing(Session::getTimeStart).thenComparing(Session::getId)));
    }

    public static long completedMinutes(Session session) {
        return Duration.between(session.getTimeStart(), session.getTimeEnd()).toMinutes();
    }

    /** Q07: Phiên hoàn tất ít nhất minimumMinutes phút, dài nhất trước. */
    public List<Session> findCompletedAtLeastMinutes(long minimumMinutes) {
        if (minimumMinutes < 0) throw new ValidationException("Minutes must be non-negative");
        return result(values.stream().filter(s -> s.getSessionStatus() == SessionStatus.COMPLETED)
            .filter(s -> completedMinutes(s) >= minimumMinutes)
            .sorted(Comparator.comparingLong(SessionDAO::completedMinutes).reversed().thenComparing(Session::getId)));
    }

    /** Q08: Phút của phiên COMPLETED bắt đầu trong ngày, từng máy kể cả 0. */
    public List<ComputerMinutes> completedMinutesByComputerOn(LocalDate day) {
        LocalDateTime start = Checks.required(day, "day").atStartOfDay();
        LocalDateTime end = start.plusDays(1);
        Map<String, Long> minutes = values.stream()
            .filter(s -> s.getSessionStatus() == SessionStatus.COMPLETED)
            .filter(s -> !s.getTimeStart().isBefore(start) && s.getTimeStart().isBefore(end))
            .collect(groupingBy(s -> s.getComputer().getId(), summingLong(SessionDAO::completedMinutes)));
        return result(computers.stream().sorted(Comparator.comparing(Computer::getId))
            .map(c -> new ComputerMinutes(c, minutes.getOrDefault(c.getId(), 0L))));
    }

    /** Q10: Tạm tính ACTIVE; báo lỗi nếu at trước bất kỳ phiên đang chạy nào. */
    public List<SessionCharge> estimateActiveAmounts(LocalDateTime at) {
        if (at == null) throw new InvalidTimeException("Estimate time is required");
        return result(values.stream().filter(s -> s.getSessionStatus() == SessionStatus.ACTIVE)
            .sorted(Comparator.comparing(Session::getId)).map(s -> new SessionCharge(s, s.amountAt(at))));
    }

    /** Q11: Lịch sử khách trên tất cả tài khoản; mới nhất trước. */
    public List<Session> historyByCustomer(String customerId) {
        String key = Checks.id(customerId);
        return result(values.stream().filter(s -> s.getAccount().getCustomer().getId().equals(key))
            .sorted(newestFirst()));
    }

    /** Q17: Phiên hoàn tất không có lần gọi món đã ghi nhận. */
    public List<Session> findCompletedWithoutFood() {
        return result(values.stream().filter(s -> s.getSessionStatus() == SessionStatus.COMPLETED)
            .filter(s -> services.stream().noneMatch(f -> f.getSession().getId().equals(s.getId())))
            .sorted(Comparator.comparing(Session::getId)));
    }

    /** Q30: Phiên hoàn tất chưa có hóa đơn. */
    public List<Session> findCompletedWithoutReceipt() {
        return result(values.stream().filter(s -> s.getSessionStatus() == SessionStatus.COMPLETED)
            .filter(s -> receipts.stream().noneMatch(r -> r.getSession().getId().equals(s.getId())))
            .sorted(Comparator.comparing(Session::getId)));
    }
}
