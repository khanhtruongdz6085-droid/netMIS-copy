package netcafe.dao;

import netcafe.Service.FoodService;
import netcafe.model.Checks;
import netcafe.model.FoodOrderDetail;
import netcafe.model.NetSpecs.Food;
import netcafe.dao.QueryResults.FoodQuantity;
import java.math.BigDecimal;
import java.util.Comparator;
import java.util.List;
import java.util.Map;
import static java.util.stream.Collectors.groupingBy;
import static java.util.stream.Collectors.summingLong;

public final class FoodDAO extends ListDAO<Food> {
    private final List<FoodService> services;

    public FoodDAO(List<Food> foods, List<FoodService> services) {
        super(foods);
        this.services = view(services);
    }

    /** Q14: Giá hiện tại <= ngưỡng, giá tăng dần. */
    public List<Food> findAtMostPrice(BigDecimal threshold) {
        Checks.money(threshold);
        return result(values.stream().filter(f -> f.getPrice().compareTo(threshold) <= 0)
            .sorted(Comparator.comparing(Food::getPrice).thenComparing(Food::getId)));
    }

    /** Q18: Món có tổng số lượng lớn nhất, giữ mọi đồng hạng, gồm phiên ACTIVE. */
    public List<FoodQuantity> findMostOrdered() {
        Map<String, Long> quantities = services.stream().flatMap(s -> s.getDetails().stream())
            .collect(groupingBy(d -> d.getFood().getId(), summingLong(FoodOrderDetail::getOrderQuantity)));
        long maximum = quantities.values().stream().mapToLong(Long::longValue).max().orElse(0);
        return result(values.stream().filter(f -> quantities.containsKey(f.getId()))
            .filter(f -> quantities.get(f.getId()) == maximum).sorted(Comparator.comparing(Food::getId))
            .map(f -> new FoodQuantity(f, quantities.get(f.getId()))));
    }
}
