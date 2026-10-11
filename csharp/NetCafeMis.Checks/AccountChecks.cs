namespace NetCafeMis.Checks;

/** Independent account registration and session checks; run Run() explicitly. */
public static class AccountChecks
{
    private static int checks;
    internal static int Count => checks;

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


    private static CustomerAccount CreateAccount(string? id, string? username, Customer? customer)
    {
        return new CustomerAccount(id, username, "test-hash", 0m, AccountStatus.ACTIVE, customer);
    }

    public static void Run()
    {
        checks = 0;
        NetCafe cafe = SampleData.Sample();
        Customer owner = cafe.Customers[3];
        DateTime start = new DateTime(2026, 10, 5, 13, 0, 0);
        Computer pc = cafe.Computers[5];
        CustomerAccount value = CreateAccount(" A05 ", " Player ", owner);
        Equal("A05", value.AccountId);
        Equal("Player", value.Username);
        Equal(true, value.Customer == owner);
        Equal(4, cafe.Accounts.Count);
        Equal(true, cafe.Sessions[3].Account == cafe.Accounts[0]);
        Fails(typeof(ValidationException), () => CreateAccount(" ", "player", owner));
        Fails(typeof(ValidationException), () => CreateAccount("A05", " ", owner));
        Fails(typeof(ValidationException), () => CreateAccount("A05", null, owner));
        Fails(typeof(ValidationException), () => CreateAccount("A05", "player", null));
        Fails(typeof(ValidationException), () => new CustomerAccount("A05", "player", null, 0m, AccountStatus.ACTIVE, owner));
        Fails(typeof(ValidationException), () => new CustomerAccount("A05", "player", " ", 0m, AccountStatus.ACTIVE, owner));
        Fails(typeof(ValidationException), () => new CustomerAccount("A05", "player", "test-hash", null, AccountStatus.ACTIVE, owner));
        Fails(typeof(ValidationException), () => new CustomerAccount("A05", "player", "test-hash", (decimal)(-1), AccountStatus.ACTIVE, owner));
        Fails(typeof(ValidationException), () => new CustomerAccount("A05", "player", "test-hash", 0m, null, owner));
        Fails(typeof(ValidationException), () => value.SetBalance((decimal)(-1)));
        Fails(typeof(ValidationException), () => value.SetBalance(null));
        Equal(0, value.Balance);
        value.SetBalance((decimal)(50000));
        Equal(50000, value.Balance);
        Fails(typeof(ValidationException), () => value.SetPasswordHash(null));
        Fails(typeof(ValidationException), () => value.SetPasswordHash(" "));
        Equal("test-hash", value.PasswordHash);
        value.SetPasswordHash("updated-test-hash");
        Equal("updated-test-hash", value.PasswordHash);
        Fails(typeof(ValidationException), () => value.SetStatus(null));
        Equal(AccountStatus.ACTIVE, value.Status);

        Fails(typeof(ValidationException), () => cafe.AddAccount(null));
        Fails(typeof(EntityNotFoundException), () => cafe.AddAccount(CreateAccount("A05", "player", new NormalCustomer("C99", "Unknown"))));
        Fails(typeof(EntityNotFoundException), () => cafe.AddAccount(CreateAccount("A05", "player", new NormalCustomer(owner.Id, owner.Name))));
        cafe.CatalogService.AddAccount(value);
        Fails(typeof(DuplicateIdException), () => cafe.AddAccount(CreateAccount(" A05 ", "other", owner)));
        Fails(typeof(DuplicateUsernameException), () => cafe.AddAccount(CreateAccount("A06", "Player", owner)));
        Fails(typeof(DuplicateUsernameException), () => cafe.AddAccount(CreateAccount("A06", " pLaYeR ", owner)));
        Equal(5, cafe.Accounts.Count);
        Fails(typeof(NotSupportedException), () => ((IList<CustomerAccount>)cafe.Accounts).Clear());

        Fails(typeof(ValidationException), () => cafe.OpenSession("S05", null, pc, start));
        Fails(typeof(ValidationException), () => new Session("S05", null, pc, start));
        // A separately constructed object with the same account ID is still unregistered.
        Fails(typeof(EntityNotFoundException), () => cafe.OpenSession("S05", CreateAccount("A05", "Player", owner), pc, start));
        value.SetStatus(AccountStatus.INACTIVE);
        Fails(typeof(InactiveAccountException), () => cafe.OpenSession("S05", value, pc, start));
        Equal(4, cafe.Sessions.Count);
        Equal(ComputerStatus.AVAILABLE, pc.ComputerStatus);
        Equal(50000, value.Balance);
        value.SetStatus(AccountStatus.ACTIVE);
        Session session = cafe.SessionService.OpenSession("S05", value, pc, start);
        Equal(true, session.Account == value);
        Equal(ComputerStatus.IN_USE, pc.ComputerStatus);
        Fails(typeof(ActiveSessionExistsException), () => cafe.OpenSession("S06", value, cafe.Computers[0], start));
        Equal(5, cafe.Sessions.Count);
        Equal(ComputerStatus.AVAILABLE, cafe.Computers[0].ComputerStatus);
        cafe.EndSession(session, start.AddHours(1));
        Fails(typeof(InvalidTimeException), () => cafe.OpenSession("S06", value, cafe.Computers[0], start.AddMinutes(30)));
        Equal(5, cafe.Sessions.Count);
        Equal(ComputerStatus.AVAILABLE, cafe.Computers[0].ComputerStatus);
        Session next = cafe.OpenSession("S06", value, pc, start.AddHours(1));
        Equal(true, next.Account == value);

        // Restrictions are per account; the requirements do not limit accounts per owner.
        CustomerAccount second = CreateAccount("A06", "second-player", owner);
        cafe.AddAccount(second);
        Session parallel = cafe.OpenSession("S07", second, cafe.Computers[0], start.AddHours(1));
        Equal(true, parallel.Account.Customer == next.Account.Customer);
        Equal(0, second.Balance);
        Equal(50000, value.Balance);
        cafe.EndSession(next, start.AddHours(2));
        cafe.EndSession(parallel, start.AddHours(2));
        Receipt receipt = cafe.IssueReceipt("R03", next, cafe.Employees[0]);
        Equal("NORMAL", receipt.CustomerTier);
        Equal(20000, receipt.TotalAmount);
        Equal(50000, value.Balance);
        Console.WriteLine("AccountChecks OK: " + checks + " checks");
    }
}
