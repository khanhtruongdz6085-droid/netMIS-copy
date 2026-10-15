package netcafe.dao;

import netcafe.Service.FoodService;
import netcafe.model.Checks;
import netcafe.model.FoodOrderDetail;
import netcafe.model.Session;
import netcafe.dao.QueryResults.SessionFoodTotal;
import java.math.BigDecimal;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import static java.util.stream.Collectors.groupingBy;
import static java.util.stream.Collectors.reducing;

public final class FoodServiceDAO extends ListDAO<FoodService> {
    private final List<Session> sessions;

    public FoodServiceDAO(List<FoodService> services, List<Session> sessions) {
        super(services);
        this.sessions = view(sessions);
    }

    /** Q15: Dòng món của phiên, gồm đơn/nhân viên/món/giá lịch sử/thành tiền. */
    public List<FoodOrderDetail> orderDetailsBySession(String sessionId) {
        String key = Checks.id(sessionId);
        return result(values.stream().filter(s -> s.getSession().getId().equals(key))
            .flatMap(s -> s.getDetails().stream())
            .sorted(Comparator.comparing((FoodOrderDetail d) -> d.getService().getTimeStartService())
                .thenComparing(d -> d.getService().getId()).thenComparing(d -> d.getFood().getId())));
    }

    /** Q16: Tổng tiền món gốc của từng phiên, kể cả phiên không gọi món. */
    public List<SessionFoodTotal> totalsBySession() {
        Map<String, BigDecimal> totals = values.stream().collect(groupingBy(s -> s.getSession().getId(),
            reducing(BigDecimal.ZERO, FoodService::getTotal, BigDecimal::add)));
        return result(sessions.stream().map(s -> new SessionFoodTotal(s, totals.getOrDefault(s.getId(), BigDecimal.ZERO)))
            .sorted(Comparator.comparing(SessionFoodTotal::getAmount).reversed()
                .thenComparing(r -> r.getSession().getId())));
    }

    /** Q19: Lần gọi món có tổng lớn hơn ngưỡng; khách qua session.account.customer. */
    public List<FoodService> findAboveTotal(BigDecimal threshold) {
        Checks.money(threshold);
        return result(values.stream().filter(s -> s.getTotal().compareTo(threshold) > 0)
            .sorted(Comparator.comparing(FoodService::getTotal).reversed().thenComparing(FoodService::getId)));
    }
}
