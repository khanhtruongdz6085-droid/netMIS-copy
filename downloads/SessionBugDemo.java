package netcafe;

import netcafe.Service.NetCafe;
import netcafe.enums.ComputerStatus;
import netcafe.enums.EmployeeStatus;
import netcafe.model.Session;
import netcafe.model.NetSpecs.Computer;
import netcafe.model.NetSpecs.ComputerSection;
import netcafe.model.NetSpecs.Employee;
import netcafe.model.customer.Customer;

import java.math.BigDecimal;
import java.time.Duration;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CyclicBarrier;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;

/** Put this file in src/main/java/netcafe, then run SessionBugDemo.main(). */
public final class SessionBugDemo {
    private static final LocalDateTime START = LocalDateTime.of(2026, 10, 17, 15, 0);

    private static NetCafe createCafe() {
        NetCafe cafe = new NetCafe();
        ComputerSection section = new ComputerSection("K1", "Khu thu", new BigDecimal("12000"));
        cafe.addSection(section);
        cafe.addComputer(new Computer("PC1", section, ComputerStatus.AVAILABLE));
        cafe.addEmployee(new Employee("E1", "Nhan vien", EmployeeStatus.ACTIVE));
        for (int i = 1; i <= 8; i++)
            cafe.addCustomer(new Customer("C" + i, "Khach " + i));
        return cafe;
    }

    // Every task is a different customer requesting the SAME computer.
    private static void demoConcurrentOpen() throws Exception {
        ExecutorService pool = Executors.newFixedThreadPool(8);
        int failedRounds = 0;
        String example = null;
        try {
            for (int round = 1; round <= 100; round++) {
                NetCafe cafe = createCafe();
                CyclicBarrier gate = new CyclicBarrier(8);
                List<Future<String>> tasks = new ArrayList<>();
                for (int i = 0; i < 8; i++) {
                    final int index = i;
                    tasks.add(pool.submit(() -> {
                        gate.await(10, TimeUnit.SECONDS);
                        try {
                            cafe.openSession("S" + index, cafe.getCustomers().get(index),
                                    cafe.getComputers().get(0), cafe.getEmployees().get(0), START);
                            return "OK";
                        } catch (RuntimeException error) {
                            return error.getClass().getSimpleName();
                        }
                    }));
                }
                int success = 0;
                List<String> results = new ArrayList<>();
                for (Future<String> task : tasks) {
                    String result = task.get(15, TimeUnit.SECONDS);
                    results.add(result);
                    if ("OK".equals(result)) success++;
                }
                if (success > 1) {
                    failedRounds++;
                    if (example == null)
                        example = "success=" + success + ", stored=" + cafe.getSessions().size()
                                + ", results=" + results;
                }
            }
        } finally {
            pool.shutdownNow();
        }
        System.out.println("1) Concurrent open: failed rounds=" + failedRounds + "/100");
        System.out.println(example == null
                ? "Not reproduced in this run; thread scheduling varies."
                : "Example: " + example);
    }

    private static void demoAfterEnd() {
        NetCafe cafe = createCafe();
        Session session = cafe.openSession("S1", cafe.getCustomers().get(0),
                cafe.getComputers().get(0), cafe.getEmployees().get(0), START);
        cafe.endSession(session, START.plusHours(1)); // 16:00
        System.out.println("2) Ended at 16:00, price=12000/hour:");
        System.out.println("amountAt(17:00)=" + session.amountAt(START.plusHours(2)));
        System.out.println("calculateAmount()=" + session.calculateAmount());
        System.out.println("Receipt computer amount=" + cafe.issueReceipt("R1", session,
                cafe.getEmployees().get(0)).getComputerAmount());
    }

    private static void demoSubsecond() {
        NetCafe cafe = createCafe();
        Session session = cafe.openSession("S1", cafe.getCustomers().get(0),
                cafe.getComputers().get(0), cafe.getEmployees().get(0), START);
        cafe.endSession(session, START.plusNanos(900_000_000)); // 0.9 seconds
        System.out.println("3) Duration=0.9 seconds, price=12000/hour:");
        System.out.println("Duration.getSeconds()="
                + Duration.between(session.getTimeStart(), session.getTimeEnd()).getSeconds());
        System.out.println("calculateAmount()=" + session.calculateAmount());
        System.out.println("Proportional charge: 12000 * 0.9 / 3600 = 3");
    }

    public static void main(String[] args) throws Exception {
        demoConcurrentOpen();
        demoAfterEnd();
        demoSubsecond();
    }
}
