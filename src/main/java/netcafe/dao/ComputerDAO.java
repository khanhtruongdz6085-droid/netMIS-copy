package netcafe.dao;

import netcafe.enums.ComputerStatus;
import netcafe.model.NetSpecs.Computer;
import netcafe.model.Session;
import java.util.Comparator;
import java.util.List;

public final class ComputerDAO extends ListDAO<Computer> {
    private final List<Session> sessions;

    public ComputerDAO(List<Computer> computers, List<Session> sessions) {
        super(computers);
        this.sessions = view(sessions);
    }

    /** Q01: Máy trống, kèm khu/giá giờ, giá tăng dần. */
    public List<Computer> findAvailable() {
        return result(values.stream().filter(c -> c.getComputerStatus() == ComputerStatus.AVAILABLE)
            .sorted(Comparator.comparing((Computer c) -> c.getSection().getSectionPrice())
                .thenComparing(Computer::getId)));
    }

    /** Q02: Máy bảo trì, kèm khu qua getSection(). */
    public List<Computer> findMaintenance() {
        return result(values.stream().filter(c -> c.getComputerStatus() == ComputerStatus.MAINTENANCE)
            .sorted(Comparator.comparing(Computer::getId)));
    }

    /** Q09: Máy chưa từng có phiên. */
    public List<Computer> findNeverUsed() {
        return result(values.stream().filter(c -> sessions.stream()
            .noneMatch(s -> s.getComputer().getId().equals(c.getId())))
            .sorted(Comparator.comparing(Computer::getId)));
    }
}
