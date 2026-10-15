package netcafe.dao;

import netcafe.enums.ComputerStatus;
import netcafe.model.Checks;
import netcafe.model.NetSpecs.Computer;
import netcafe.model.NetSpecs.ComputerSection;
import netcafe.dao.QueryResults.ComputerCount;
import java.math.BigDecimal;
import java.util.Arrays;
import java.util.Comparator;
import java.util.List;

public final class ComputerSectionDAO extends ListDAO<ComputerSection> {
    private final List<Computer> computers;

    public ComputerSectionDAO(List<ComputerSection> sections, List<Computer> computers) {
        super(sections);
        this.computers = view(computers);
    }

    /** Q03: Mọi trạng thái trong mọi khu, kể cả khu chưa có máy. */
    public List<ComputerCount> countComputersByStatus() {
        return result(values.stream().sorted(Comparator.comparing(ComputerSection::getId))
            .flatMap(section -> Arrays.stream(ComputerStatus.values())
                .sorted(Comparator.comparing(Enum::name))
                .map(status -> new ComputerCount(section, status, computers.stream()
                    .filter(c -> c.getSection().getId().equals(section.getId()))
                    .filter(c -> c.getComputerStatus() == status).count()))));
    }

    /** Q04: Khu có giá giờ lớn hơn ngưỡng; giá giảm dần. */
    public List<ComputerSection> findAboveHourlyPrice(BigDecimal threshold) {
        Checks.money(threshold);
        return result(values.stream().filter(s -> s.getSectionPrice().compareTo(threshold) > 0)
            .sorted(Comparator.comparing(ComputerSection::getSectionPrice).reversed()
                .thenComparing(ComputerSection::getId)));
    }
}
