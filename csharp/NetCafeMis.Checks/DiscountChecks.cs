namespace NetCafeMis.Checks;

public static class DiscountChecks
{
    private static int checks;
    internal static int Count => checks;
    private static decimal Money(long value) { return (decimal)(value); }
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

    private static IDiscount Policy(decimal? computerRate, decimal? foodRate) =>
        new TestPolicy(computerRate, foodRate);

    private sealed class TestPolicy(decimal? computerRate, decimal? foodRate) : IDiscount
    {
        public string TierName => "CUSTOM";
        public decimal? ComputerDiscountRate => computerRate;
        public decimal? FoodDiscountRate => foodRate;
    }

    private sealed class TestCustomer : Customer
    {
        public override string TierName => "TEST";
        public override decimal? ComputerDiscountRate => computerRate;
        // Mutable fields simulate a policy changing after a receipt is issued.
        private decimal? computerRate;
        private decimal? foodRate;
        public override decimal? FoodDiscountRate => foodRate;

        internal TestCustomer(string id, decimal? computerRate, decimal? foodRate)
            : base(id, "Test customer") => SetRates(computerRate, foodRate);

        internal void SetRates(decimal? computerRate, decimal? foodRate)
        {
            this.computerRate = computerRate;
            this.foodRate = foodRate;
        }
    }

    public static void Run()
    {
        checks = 0;
        NetCafe cafe = DiscountDemo.Sample();
        Receipt normal = cafe.Receipts[0], member = cafe.Receipts[1], vip = cafe.Receipts[2];
        Equal("NORMAL", normal.CustomerTier);
        Equal(0, normal.TotalDiscountAmount);
        Equal(150000, normal.TotalAmount);
        Equal("MEMBER", member.CustomerTier);
        Equal(100000, member.GrossComputerAmount);
        Equal(50000, member.GrossServiceAmount);
        Equal("0.10", member.ComputerDiscountRate);
        Equal("0.05", member.ServiceDiscountRate);
        Equal(10000, member.ComputerDiscountAmount);
        Equal(2500, member.ServiceDiscountAmount);
        Equal(90000, member.ComputerAmount);
        Equal(47500, member.ServiceAmount);
        Equal(137500, member.CalculateAmount());
        Equal(15000, vip.ComputerDiscountAmount);
        Equal(4000, vip.ServiceDiscountAmount);
        Equal(131000, vip.TotalAmount);
        Equal(418500, cafe.CollectedAmount());

        Equal(true, normal.Session.Account.Customer is NormalCustomer);
        Equal(true, member.Session.Account.Customer is MemberCustomer);
        Equal(true, vip.Session.Account.Customer is VipCustomer);
        CustomerAccount account = vip.Session.Account;
        Customer customer = account.Customer;
        Equal("VIP", customer.TierName);
        Equal("MEMBER", member.CustomerTier);
        Equal(137500, member.TotalAmount);
        DateTime start = new DateTime(2026, 10, 5, 11, 0, 0);
        Session vipSession = cafe.OpenSession("S04", account, cafe.Computers[0], start);
        cafe.EndSession(vipSession, start.AddHours(1));
        Receipt vipReceipt = cafe.IssueReceipt("R04", vipSession, cafe.Employees[0]);
        Equal("VIP", vipReceipt.CustomerTier);
        Equal(85000, vipReceipt.TotalAmount);
        Fails(typeof(PaymentAmountMismatchException), () => cafe.Pay("P04", vipReceipt, start.AddHours(1), PaymentMethod.CASH, Money(100000)));
        Equal(3, cafe.Payments.Count);
        cafe.Pay("P04", vipReceipt, start.AddHours(1), PaymentMethod.CASH, Money(85000));
        Equal(503500, cafe.CollectedAmount());

        Receipt rounded = new Receipt("ROUND", member.Session, cafe.Employees[0], Money(10), Money(5));
        Equal(1, rounded.ComputerDiscountAmount);
        Equal(1, rounded.ServiceDiscountAmount);
        Equal(13, rounded.TotalAmount);
        Receipt free = new Receipt("FREE", vipSession, cafe.Employees[0], Money(10), Money(5), Policy(1m, 1m));
        Equal(0, free.TotalAmount);
        Receipt zero = new Receipt("ZERO", vipSession, cafe.Employees[0], 0m, 0m);
        Equal(0, zero.TotalAmount);

        NetCafe invalid = SampleData.Sample();
        TestCustomer testCustomer = new TestCustomer("C99", 0.10m, 0.05m);
        invalid.AddCustomer(testCustomer);
        CustomerAccount testAccount = new CustomerAccount("A99", "testcustomer", "test-hash", Money(0), AccountStatus.ACTIVE, testCustomer);
        invalid.AddAccount(testAccount);
        Session session = invalid.OpenSession("S99", testAccount, invalid.Computers[5], start.AddHours(3));
        invalid.EndSession(session, start.AddHours(4));
        Employee employee = invalid.Employees[0];
        Receipt frozen = new Receipt("FROZEN", session, employee, Money(50000), Money(100000));
        testCustomer.SetRates(0.15m, 0.08m);
        Equal(137500, frozen.TotalAmount);
        Equal("0.10", frozen.ComputerDiscountRate);
        Equal("0.05", frozen.ServiceDiscountRate);
        testCustomer.SetRates(1.01m, 0m);
        Fails(typeof(ValidationException), () => invalid.IssueReceipt("R03", session, employee));
        testCustomer.SetRates(0m, -0.01m);
        Fails(typeof(ValidationException), () => invalid.IssueReceipt("R03", session, employee));
        testCustomer.SetRates(null, 0m);
        Fails(typeof(ValidationException), () => invalid.IssueReceipt("R03", session, employee));
        Equal(2, invalid.Receipts.Count);
        Fails(typeof(ValidationException), () => new Receipt("NULL", session, employee, Money(10), Money(5), null));
        testCustomer.SetRates(0.15m, 0.08m);
        Equal(17000, invalid.IssueReceipt("R03", session, employee).TotalAmount);
        Console.WriteLine("DiscountChecks OK: " + checks + " checks");
    }
}
