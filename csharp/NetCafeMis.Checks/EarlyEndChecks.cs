namespace NetCafeMis.Checks;

/** Customers end sessions when they leave; billing and payment use the actual end. */
public static class EarlyEndChecks
{
    private static int checks;
    internal static int Count => checks;
    private static readonly DateTime START = new DateTime(2026, 10, 5, 3, 0, 0);
    private static readonly DateTime TWO_HOURS_LATER = START.AddHours(2);

    private static void Equal(object? expected, object? actual)
    {
        checks++;
        CheckAssert.Equal(expected, actual);
    }

    private static void Fails(Type type, Action action)
    {
        checks++;
        CheckAssert.Fails(type, action);
    }


    private sealed class Fixture
    {
        internal readonly NetCafe cafe = new NetCafe();
        internal readonly ComputerSection section = new ComputerSection("CS01", "Demo", (decimal)(20000));
        internal readonly Computer computer;
        internal readonly Employee employee = new Employee("E01", "Cashier", EmployeeStatus.ACTIVE);
        internal readonly CustomerAccount account;

        internal Fixture(Customer customer)
        {
            computer = new Computer("PC01", section, ComputerStatus.AVAILABLE);
            account = new CustomerAccount("A01", "test", "test-hash", 0m, AccountStatus.ACTIVE, customer);
            cafe.AddSection(section);
            cafe.AddComputer(computer);
            cafe.AddCustomer(customer);
            cafe.AddAccount(account);
            cafe.AddEmployee(employee);
        }

        internal Session Open()
        {
            return cafe.OpenSession("S01", account, computer, START);
        }
    }

    public static void Run()
    {
        checks = 0;
        Fixture early = new Fixture(new NormalCustomer("C01", "Normal"));
        Session session = early.Open();
        Equal(null, session.TimeEnd);
        Equal(SessionStatus.ACTIVE, session.SessionStatus);
        Equal(ComputerStatus.IN_USE, early.computer.ComputerStatus);
        Fails(typeof(SessionStateException), () => early.cafe.IssueReceipt("R01", session, early.employee));
        Equal(0, early.cafe.Receipts.Count);
        Receipt premature = new Receipt("UNREGISTERED", session, early.employee, 0m, 0m);
        Fails(typeof(SessionStateException), () => new Payment("P01", premature, TWO_HOURS_LATER, PaymentMethod.CASH, 0m));
        // Estimating accrued cost does not complete the session.
        Equal(40333, session.AmountAt(TWO_HOURS_LATER.AddMinutes(1)));
        Equal(null, session.TimeEnd);
        Equal(SessionStatus.ACTIVE, session.SessionStatus);

        DateTime actualEnd = START.AddMinutes(90);
        early.cafe.EndSession(session, actualEnd);
        Equal(actualEnd, session.TimeEnd);
        Equal(SessionStatus.COMPLETED, session.SessionStatus);
        Equal(ComputerStatus.AVAILABLE, early.computer.ComputerStatus);
        Receipt receipt = early.cafe.IssueReceipt("R01", session, early.employee);
        Equal(30000, receipt.GrossComputerAmount);
        Equal(30000, receipt.TotalAmount);
        Fails(typeof(InvalidTimeException), () => early.cafe.Pay("P01", receipt, actualEnd.AddMinutes(-1), PaymentMethod.CASH, receipt.TotalAmount));
        Equal(0, early.cafe.Payments.Count);
        Equal(0, early.cafe.CollectedAmount());
        Payment payment = early.cafe.Pay("P01", receipt, actualEnd, PaymentMethod.CASH, receipt.TotalAmount);
        Equal(actualEnd, payment.Time);
        Equal(30000, early.cafe.CollectedAmount());
        Fails(typeof(SessionStateException), () => early.cafe.EndSession(session, TWO_HOURS_LATER));
        Equal(actualEnd, session.TimeEnd);
        Session next = early.cafe.OpenSession("S02", early.account, early.computer, actualEnd);
        Equal(SessionStatus.ACTIVE, next.SessionStatus);

        Fixture onTime = new Fixture(new NormalCustomer("C02", "Two hours"));
        Session onTimeSession = onTime.Open();
        onTime.cafe.EndSession(onTimeSession, TWO_HOURS_LATER);
        Receipt onTimeReceipt = onTime.cafe.IssueReceipt("R01", onTimeSession, onTime.employee);
        Equal(40000, onTimeReceipt.TotalAmount);
        onTime.cafe.Pay("P01", onTimeReceipt, TWO_HOURS_LATER.AddMinutes(1), PaymentMethod.CASH, onTimeReceipt.TotalAmount);
        Equal(40000, onTime.cafe.CollectedAmount());

        Fixture late = new Fixture(new NormalCustomer("C03", "Longer session"));
        Session lateSession = late.Open();
        DateTime lateEnd = TWO_HOURS_LATER.AddMinutes(30);
        late.cafe.EndSession(lateSession, lateEnd);
        Receipt lateReceipt = late.cafe.IssueReceipt("R01", lateSession, late.employee);
        Equal(50000, lateReceipt.TotalAmount);
        Fails(typeof(InvalidTimeException), () => late.cafe.OpenSession("S02", late.account, late.computer, TWO_HOURS_LATER.AddMinutes(15)));
        Equal(1, late.cafe.Sessions.Count);
        Equal(ComputerStatus.AVAILABLE, late.computer.ComputerStatus);
        late.cafe.Pay("P01", lateReceipt, lateEnd, PaymentMethod.CASH, lateReceipt.TotalAmount);
        Equal(50000, late.cafe.CollectedAmount());

        Fixture ordered = new Fixture(new NormalCustomer("C04", "Food order"));
        Session orderedSession = ordered.Open();
        Food food = new Food("F01", "Combo", (decimal)(10000));
        ordered.cafe.AddFood(food);
        DateTime orderTime = START.AddMinutes(100);
        ordered.cafe.Order("SV01", orderedSession, ordered.employee, orderTime, new Food[] { food }, new int[] { 1 });
        Fails(typeof(InvalidTimeException), () => ordered.cafe.EndSession(orderedSession, actualEnd));
        Equal(null, orderedSession.TimeEnd);
        Equal(SessionStatus.ACTIVE, orderedSession.SessionStatus);
        Equal(ComputerStatus.IN_USE, ordered.computer.ComputerStatus);
        ordered.cafe.EndSession(orderedSession, orderTime);
        Equal(43333, ordered.cafe.IssueReceipt("R01", orderedSession, ordered.employee).TotalAmount);

        Customer[] tiers = { new MemberCustomer("C05", "Member"), new VipCustomer("C06", "VIP") };
        long[] expected = { 27000, 25500 };
        for (int i = 0; i < tiers.Length; i++)
        {
            Fixture discounted = new Fixture(tiers[i]);
            Session discountedSession = discounted.Open();
            discounted.cafe.EndSession(discountedSession, actualEnd);
            Receipt discountedReceipt = discounted.cafe.IssueReceipt("R01", discountedSession, discounted.employee);
            Equal(30000, discountedReceipt.GrossComputerAmount);
            Equal(expected[i], discountedReceipt.TotalAmount);
            discounted.cafe.Pay("P01", discountedReceipt, actualEnd, PaymentMethod.CASH, discountedReceipt.TotalAmount);
            Equal(expected[i], discounted.cafe.CollectedAmount());
        }
        Console.WriteLine("EarlyEndChecks OK: " + checks + " checks");
    }
}
