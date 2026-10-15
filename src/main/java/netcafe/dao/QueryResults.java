package netcafe.dao;

import netcafe.enums.ComputerStatus;
import netcafe.enums.PaymentMethod;
import netcafe.model.Receipt;
import netcafe.model.Session;
import netcafe.model.NetSpecs.Computer;
import netcafe.model.NetSpecs.ComputerSection;
import netcafe.model.NetSpecs.Employee;
import netcafe.model.NetSpecs.Food;
import netcafe.model.customer.Customer;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/** Immutable result rows for aggregate queries. */
public final class QueryResults {
    private QueryResults() {}

    public static final class ComputerCount {
        private final ComputerSection section;
        private final ComputerStatus status;
        private final long count;

        ComputerCount(ComputerSection section, ComputerStatus status, long count) {
            this.section = section;
            this.status = status;
            this.count = count;
        }
        public ComputerSection getSection() { return section; }
        public ComputerStatus getStatus() { return status; }
        public long getCount() { return count; }
    }

    public static final class ComputerMinutes {
        private final Computer computer;
        private final long minutes;

        ComputerMinutes(Computer computer, long minutes) {
            this.computer = computer;
            this.minutes = minutes;
        }
        public Computer getComputer() { return computer; }
        public long getMinutes() { return minutes; }
    }

    public static final class SessionCharge {
        private final Session session;
        private final BigDecimal amount;

        SessionCharge(Session session, BigDecimal amount) {
            this.session = session;
            this.amount = amount;
        }
        public Session getSession() { return session; }
        public BigDecimal getAmount() { return amount; }
    }

    public static final class CustomerSessions {
        private final Customer customer;
        private final long count;

        CustomerSessions(Customer customer, long count) {
            this.customer = customer;
            this.count = count;
        }
        public Customer getCustomer() { return customer; }
        public long getCount() { return count; }
    }

    public static final class SessionFoodTotal {
        private final Session session;
        private final BigDecimal amount;

        SessionFoodTotal(Session session, BigDecimal amount) {
            this.session = session;
            this.amount = amount;
        }
        public Session getSession() { return session; }
        public BigDecimal getAmount() { return amount; }
    }

    public static final class FoodQuantity {
        private final Food food;
        private final long quantity;

        FoodQuantity(Food food, long quantity) {
            this.food = food;
            this.quantity = quantity;
        }
        public Food getFood() { return food; }
        public long getQuantity() { return quantity; }
    }

    public static final class EmployeeFoodStats {
        private final Employee employee;
        private final long count;
        private final BigDecimal amount;

        EmployeeFoodStats(Employee employee, long count, BigDecimal amount) {
            this.employee = employee;
            this.count = count;
            this.amount = amount;
        }
        public Employee getEmployee() { return employee; }
        public long getCount() { return count; }
        public BigDecimal getAmount() { return amount; }
    }

    public static final class DailyRevenue {
        private final LocalDate day;
        private final BigDecimal amount;

        DailyRevenue(LocalDate day, BigDecimal amount) {
            this.day = day;
            this.amount = amount;
        }
        public LocalDate getDay() { return day; }
        public BigDecimal getAmount() { return amount; }
    }

    public static final class MethodRevenue {
        private final PaymentMethod method;
        private final long count;
        private final BigDecimal amount;

        MethodRevenue(PaymentMethod method, long count, BigDecimal amount) {
            this.method = method;
            this.count = count;
            this.amount = amount;
        }
        public PaymentMethod getMethod() { return method; }
        public long getCount() { return count; }
        public BigDecimal getAmount() { return amount; }
    }

    public static final class CustomerAmount {
        private final Customer customer;
        private final BigDecimal amount;

        CustomerAmount(Customer customer, BigDecimal amount) {
            this.customer = customer;
            this.amount = amount;
        }
        public Customer getCustomer() { return customer; }
        public BigDecimal getAmount() { return amount; }
    }

    public static final class EmployeeBilling {
        private final Employee employee;
        private final long count;
        private final BigDecimal total;
        private final BigDecimal collected;
        private final BigDecimal outstanding;

        EmployeeBilling(Employee employee, long count, BigDecimal total, BigDecimal collected, BigDecimal outstanding) {
            this.employee = employee;
            this.count = count;
            this.total = total;
            this.collected = collected;
            this.outstanding = outstanding;
        }
        public Employee getEmployee() { return employee; }
        public long getCount() { return count; }
        public BigDecimal getTotal() { return total; }
        public BigDecimal getCollected() { return collected; }
        public BigDecimal getOutstanding() { return outstanding; }
    }

    public static final class SectionRevenue {
        private final ComputerSection section;
        private final BigDecimal computerAmount;
        private final BigDecimal foodAmount;
        private final BigDecimal collected;

        SectionRevenue(ComputerSection section, BigDecimal computerAmount, BigDecimal foodAmount, BigDecimal collected) {
            this.section = section;
            this.computerAmount = computerAmount;
            this.foodAmount = foodAmount;
            this.collected = collected;
        }
        public ComputerSection getSection() { return section; }
        public BigDecimal getComputerAmount() { return computerAmount; }
        public BigDecimal getFoodAmount() { return foodAmount; }
        public BigDecimal getCollected() { return collected; }
    }

    public static final class PaymentIssue {
        private final Receipt receipt;
        private final long count;
        private final BigDecimal paidAmount;
        private final List<String> reasons;

        PaymentIssue(Receipt receipt, long count, BigDecimal paidAmount, List<String> reasons) {
            this.receipt = receipt;
            this.count = count;
            this.paidAmount = paidAmount;
            this.reasons = Collections.unmodifiableList(new ArrayList<>(reasons));
        }
        public Receipt getReceipt() { return receipt; }
        public long getCount() { return count; }
        public BigDecimal getPaidAmount() { return paidAmount; }
        public List<String> getReasons() { return reasons; }
    }
}
