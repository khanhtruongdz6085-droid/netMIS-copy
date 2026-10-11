namespace NetCafeMis;

/** Started at 03:00; the customer ends the session and pays at 04:30. */
public static class EarlyEndDemo
{
    public static NetCafe Sample()
    {
        NetCafe cafe = new NetCafe();
        ComputerSection section = new ComputerSection("CS01", "Demo", 20000m);
        Computer computer = new Computer("PC01", section, ComputerStatus.AVAILABLE);
        Customer customer = new NormalCustomer("C01", "Early departure");
        CustomerAccount account = new CustomerAccount("A01", "early-demo", "demo-hash-not-for-login",
                0m, AccountStatus.ACTIVE, customer);
        Employee employee = new Employee("E01", "Cashier", EmployeeStatus.ACTIVE);
        cafe.AddSection(section);
        cafe.AddComputer(computer);
        cafe.AddCustomer(customer);
        cafe.AddAccount(account);
        cafe.AddEmployee(employee);

        DateTime start = new DateTime(2026, 10, 5, 3, 0, 0);
        Session session = cafe.OpenSession("S01", account, computer, start);
        DateTime actualEnd = start.AddMinutes(90);
        cafe.EndSession(session, actualEnd);
        Receipt receipt = cafe.IssueReceipt("R01", session, employee);
        cafe.Pay("P01", receipt, actualEnd, PaymentMethod.CASH, receipt.TotalAmount);
        return cafe;
    }

    public static void Run()
    {
        NetCafe cafe = Sample();
        Session session = cafe.Sessions[0];
        Receipt receipt = cafe.Receipts[0];
        Console.WriteLine("Start=" + session.TimeStart.ToString("HH:mm")
                + " ActualEnd=" + session.TimeEnd!.Value.ToString("HH:mm"));
        Console.WriteLine("Minutes=" + (long)(session.TimeEnd!.Value - session.TimeStart).TotalMinutes);
        Console.WriteLine("Total=" + receipt.TotalAmount);
        Console.WriteLine("Collected=" + cafe.CollectedAmount());
    }
}
