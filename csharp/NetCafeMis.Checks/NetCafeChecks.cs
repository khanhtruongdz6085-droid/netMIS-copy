namespace NetCafeMis.Checks;

/** Independent checks: run Run(), no JUnit dependency. */
public static class NetCafeChecks
{
    private static int checks;
    internal static int Count => checks;
    private static decimal Money(long amount) { return (decimal)(amount); }
    private static DateTime TimeAt(int hour, int minute) { return new DateTime(2026, 10, 5, hour, minute, 0); }

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


    public static void Run()
    {
        checks = 0;
        NetCafe cafe = SampleData.Sample();
        Session active = cafe.Sessions[3];
        Employee employee = cafe.Employees[0];
        Food food = cafe.Foods[0];
        Receipt paid = cafe.Receipts[0], unpaid = cafe.Receipts[1];
        Equal(4, cafe.Sessions.Count);
        Equal(3, cafe.Services.Count);
        Equal(49000, paid.TotalAmount);
        Equal(60000, unpaid.TotalAmount);
        Equal(49000, cafe.CollectedAmount());
        Equal(ComputerStatus.IN_USE, active.Computer.ComputerStatus);
        Equal(10000, cafe.Services[0].Details[1].PriceAtOrder);

        Fails(typeof(ValidationException), () => new NormalCustomer(" ", "Name"));
        Fails(typeof(ValidationException), () => new Food("F99", "Bad", Money(-1)));
        Fails(typeof(ValidationException), () => cafe.AddCustomer(null));
        Fails(typeof(DuplicateIdException), () => cafe.AddCustomer(new NormalCustomer(" C01 ", "Other")));
        CustomerAccount unknown = new CustomerAccount("A99", "unknown", "test-hash", Money(0), AccountStatus.ACTIVE, cafe.Customers[3]);
        Fails(typeof(EntityNotFoundException), () => cafe.OpenSession("S05", unknown, cafe.Computers[5], TimeAt(12, 0)));
        Fails(typeof(ComputerUnavailableException), () => cafe.OpenSession("S05", cafe.Accounts[3], cafe.Computers[4], TimeAt(12, 0)));
        Fails(typeof(ComputerUnavailableException), () => cafe.OpenSession("S05", cafe.Accounts[3], active.Computer, TimeAt(12, 0)));
        Fails(typeof(ActiveSessionExistsException), () => cafe.OpenSession("S05", active.Account, cafe.Computers[5], TimeAt(12, 0)));
        Fails(typeof(InvalidTimeException), () => cafe.OpenSession("S05", cafe.Accounts[3], cafe.Computers[0], TimeAt(9, 0)));
        Fails(typeof(InvalidTimeException), () => cafe.EndSession(active, TimeAt(11, 10)));
        Equal(SessionStatus.ACTIVE, active.SessionStatus);
        Equal(ComputerStatus.IN_USE, active.Computer.ComputerStatus);
        Fails(typeof(SessionStateException), () => cafe.EndSession(cafe.Sessions[0], TimeAt(12, 0)));
        Fails(typeof(SessionStateException), () => cafe.IssueReceipt("R03", active, employee));
        Fails(typeof(ReceiptAlreadyExistsException), () => cafe.IssueReceipt("R03", cafe.Sessions[0], employee));
        Fails(typeof(InactiveEmployeeException), () => cafe.Order("SV04", active, cafe.Employees[2], TimeAt(11, 30), new Food[] { food }, new int[] { 1 }));
        Fails(typeof(InvalidOrderException), () => cafe.Order("SV04", active, employee, TimeAt(11, 30), new Food[] { food }, new int[] { 0 }));
        Fails(typeof(InvalidOrderException), () => cafe.Order("SV04", active, employee, TimeAt(11, 30), new Food[] { food }, new int[] { 1, 2 }));
        Fails(typeof(InvalidTimeException), () => cafe.Order("SV04", active, employee, TimeAt(10, 0), new Food[] { food }, new int[] { 1 }));
        Fails(typeof(PaymentAlreadyExistsException), () => cafe.Pay("P02", paid, TimeAt(12, 0), PaymentMethod.CARD, Money(49000)));
        Fails(typeof(PaymentAmountMismatchException), () => cafe.Pay("P02", unpaid, TimeAt(12, 0), PaymentMethod.CASH, Money(59999)));
        Fails(typeof(PaymentAmountMismatchException), () => cafe.Pay("P02", unpaid, TimeAt(12, 0), PaymentMethod.CASH, Money(60001)));
        Fails(typeof(InvalidTimeException), () => cafe.Pay("P02", unpaid, TimeAt(10, 0), PaymentMethod.CASH, Money(60000)));
        Equal(1, cafe.Payments.Count);
        Equal(3, cafe.Services.Count);
        FoodService merged = cafe.Order("SV04", active, employee, TimeAt(11, 30), new Food[] { food, food }, new int[] { 1, 2 });
        Equal(1, merged.Details.Count);
        Equal(3, merged.Details[0].OrderQuantity);
        Equal(39000, merged.Total);
        food.SetPrice(Money(20000));
        Equal(39000, merged.Total);
        Fails(typeof(InvalidOrderException), () => merged.AddItem(food, 1, Money(13000)));
        Fails(typeof(InvalidOrderException), () => cafe.Order("SV05", active, employee, TimeAt(11, 40), new Food[] { food, food }, new int[] { int.MaxValue, 1 }));
        Equal(4, cafe.Services.Count);
        cafe.SessionService.EndSession(active, TimeAt(12, 0));
        Equal(ComputerStatus.AVAILABLE, active.Computer.ComputerStatus);
        Receipt finalReceipt = cafe.ReceiptService.IssueReceipt("R03", active, employee);
        Equal(71000, finalReceipt.TotalAmount);
        cafe.PaymentService.Pay("P02", finalReceipt, TimeAt(12, 0), PaymentMethod.BANK_TRANSFER, finalReceipt.TotalAmount);
        Equal(120000, cafe.CollectedAmount());
        Session rounding = new Session("S99", cafe.Accounts[3], cafe.Computers[0], TimeAt(13, 0));
        rounding.EndSession(TimeAt(13, 0).AddSeconds(1));
        Equal(3, rounding.CalculateAmount());
        Console.WriteLine("NetCafeChecks OK: " + checks + " checks");
    }
}
