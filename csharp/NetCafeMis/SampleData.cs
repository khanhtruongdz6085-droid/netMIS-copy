namespace NetCafeMis;

public static class SampleData
{
    private static decimal Money(long amount) { return (decimal)(amount); }
    private static DateTime TimeAt(int hour, int minute) { return new DateTime(2026, 10, 5, hour, minute, 0); }
    public static NetCafe Sample()
    {
        NetCafe cafe = new NetCafe();
        CatalogService catalog = cafe.CatalogService;
        SessionService sessions = cafe.SessionService;
        FoodOrderService orders = cafe.FoodOrderService;
        ReceiptService receipts = cafe.ReceiptService;
        PaymentService payments = cafe.PaymentService;
        ComputerSection
                normal = new ComputerSection("CS01", "Khu thường", Money(12000)),
                vip = new ComputerSection("CS02", "Khu VIP", Money(20000));
        catalog.AddSection(normal); catalog.AddSection(vip);

        Computer
                pc1 = new Computer("PC01", normal, ComputerStatus.AVAILABLE),
                pc2 = new Computer("PC02", normal, ComputerStatus.AVAILABLE),
                pc3 = new Computer("PC03", vip, ComputerStatus.AVAILABLE),
                pc4 = new Computer("PC04", vip, ComputerStatus.AVAILABLE);
        catalog.AddComputer(pc1);
        catalog.AddComputer(pc2);
        catalog.AddComputer(pc3);
        catalog.AddComputer(pc4);
        catalog.AddComputer(new Computer("PC05", normal, ComputerStatus.MAINTENANCE));
        catalog.AddComputer(new Computer("PC06", vip, ComputerStatus.AVAILABLE));

        Customer
                c1 = new NormalCustomer("C01", "Lamine Yamal"),
                c2 = new NormalCustomer("C02", "Bruno Fernades"),
                c3 = new NormalCustomer("C03", "Cristiano Ronaldo");
        catalog.AddCustomer(c1);
        catalog.AddCustomer(c2);
        catalog.AddCustomer(c3);
        catalog.AddCustomer(new NormalCustomer("C04", "Lionel Messi"));

        // Demo-only placeholders; no login/authentication is implemented by this sample.
        CustomerAccount a1 = new CustomerAccount("A01", "yamal", "demo-hash-not-for-login", Money(0), AccountStatus.ACTIVE, c1);
        CustomerAccount a2 = new CustomerAccount("A02", "bruno", "demo-hash-not-for-login", Money(0), AccountStatus.ACTIVE, c2);
        CustomerAccount a3 = new CustomerAccount("A03", "ronaldo", "demo-hash-not-for-login", Money(0), AccountStatus.ACTIVE, c3);
        CustomerAccount a4 = new CustomerAccount("A04", "messi", "demo-hash-not-for-login", Money(0), AccountStatus.ACTIVE, cafe.Customers[3]);
        catalog.AddAccount(a1); catalog.AddAccount(a2); catalog.AddAccount(a3); catalog.AddAccount(a4);

        Employee
                e1 = new Employee("E01", "Erik TenHag", EmployeeStatus.ACTIVE),
                e2 = new Employee("E02", "Anchelotti", EmployeeStatus.ACTIVE);
        catalog.AddEmployee(e1);
        catalog.AddEmployee(e2);
        catalog.AddEmployee(new Employee("E03", "Luis Enrique", EmployeeStatus.INACTIVE));

        Food
                f1 = new Food("F01", "Mì ly", Money(13000)),
                f2 = new Food("F02", "Nước ngọt", Money(15000));
        catalog.AddFood(f1);
        catalog.AddFood(f2);
        catalog.AddFood(new Food("F03", "Snack", Money(7000)));

        Session s1 = sessions.OpenSession("S01", a1, pc1, TimeAt(8, 0));
        orders.Order(
                "SV01", s1, e1, TimeAt(8, 20),
                new Food[] { f1, f2 },
                new int[] { 1, 1 },
                new decimal?[] { Money(15000), Money(10000) });

        Session s2 = sessions.OpenSession("S02", a2, pc3, TimeAt(9, 0));
        orders.Order(
                "SV02", s2, e2, TimeAt(9, 30),
                new Food[] { f1 },
                new int[] { 2 },
                new decimal?[] { Money(15000) });

        Session s3 = sessions.OpenSession("S03", a3, pc4, TimeAt(10, 0));

        sessions.EndSession(s1, TimeAt(10, 0));
        sessions.EndSession(s2, TimeAt(10, 30));
        sessions.EndSession(s3, TimeAt(11, 0));

        Session s4 = sessions.OpenSession("S04", a1, pc2, TimeAt(11, 0));
        orders.Order(
                "SV03", s4, e1, TimeAt(11, 15),
                new Food[] { f2 },
                new int[] { 2 },
                new decimal?[] { Money(10000) });

        Receipt r1 = receipts.IssueReceipt("R01", s1, e2),
                r2 = receipts.IssueReceipt("R02", s2, e1);
        payments.Pay("P01", r1, TimeAt(10, 5), PaymentMethod.CASH, r1.TotalAmount);
        return cafe;
    }
    public static void Run()
    {
        NetCafe cafe = Sample();
        foreach (Receipt r in cafe.Receipts) Console.WriteLine(r.ReceiptId + " session=" + r.Session.SessionId + " computer=" + r.ComputerAmount + " service=" + r.ServiceAmount + " total=" + r.TotalAmount);
        Console.WriteLine("Collected=" + cafe.CollectedAmount());
        foreach (Session s in cafe.Sessions) Console.WriteLine(s.SessionId + " " + s.SessionStatus + " " + s.Computer.ComputerId);
    }
}
