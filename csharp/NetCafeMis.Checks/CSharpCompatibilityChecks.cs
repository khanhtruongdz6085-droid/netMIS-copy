namespace NetCafeMis.Checks;

/// <summary>Check C# enum casts, decimal rounding and whole-second billing.</summary>
internal static class CSharpCompatibilityChecks
{
    internal static int Count { get; private set; }

    private static void Equal(object? expected, object? actual)
    {
        Count++;
        CheckAssert.Equal(expected, actual);
    }

    private static void Fails(Action action)
    {
        Count++;
        CheckAssert.Fails(typeof(ValidationException), action);
    }

    private sealed class FullDiscount : IDiscount
    {
        public string TierName => "CUSTOM";
        public decimal? ComputerDiscountRate => 1m;
        public decimal? FoodDiscountRate => 1m;
    }

    internal static void Run()
    {
        Count = 0;
        NetCafe cafe = SampleData.Sample();
        Computer computer = cafe.Computers[0];
        Employee employee = cafe.Employees[0];
        CustomerAccount account = cafe.Accounts[0];
        Fails(() => new Computer("BAD", computer.Section, (ComputerStatus)999));
        Fails(() => new Employee("BAD", "Name", (EmployeeStatus)999));
        Fails(() => new CustomerAccount("BAD", "name", "test-hash", 0m, (AccountStatus)999, account.Customer));
        Fails(() => new Payment("BAD", cafe.Receipts[0], new DateTime(2026, 10, 5, 12, 0, 0), (PaymentMethod)999, 49000m));
        Fails(() => computer.SetComputerStatus((ComputerStatus)999));
        Equal(ComputerStatus.AVAILABLE, computer.ComputerStatus);
        Fails(() => employee.SetEmployeeStatus((EmployeeStatus)999));
        Equal(EmployeeStatus.ACTIVE, employee.EmployeeStatus);
        Fails(() => account.SetStatus((AccountStatus)999));
        Equal(AccountStatus.ACTIVE, account.Status);

        DateTime start = new(2026, 10, 5, 14, 0, 0);
        Computer roundingComputer = new("ROUND", new ComputerSection("ROUND", "Rounding", 1800m), ComputerStatus.AVAILABLE);
        Session session = new("ROUND", account, roundingComputer, start);
        Equal(0m, session.AmountAt(start.AddMilliseconds(999)));
        Equal(1m, session.AmountAt(start.AddSeconds(1))); // 0.5 VND rounds up.
        Equal(1m, session.AmountAt(start.AddMilliseconds(1999))); // Still one whole second.
        session.EndSession(start.AddMilliseconds(1999));
        Equal(start.AddMilliseconds(1999), session.TimeEnd);
        Equal(1m, session.CalculateAmount());

        Receipt fractional = new("FRACTION", session, employee, 0.75m, 0.75m, new FullDiscount());
        Equal(0.75m, fractional.ComputerDiscountAmount); // Discount cannot exceed gross.
        Equal(0.75m, fractional.ServiceDiscountAmount);
        Equal(0m, fractional.TotalAmount);
        Console.WriteLine($"CSharpCompatibilityChecks OK: {Count} checks");
    }
}
