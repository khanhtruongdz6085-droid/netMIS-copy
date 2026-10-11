namespace NetCafeMis;

/** Compare the three tiers using the same gross computer/food amounts. */
public static class DiscountDemo
{
    public static NetCafe Sample()
    {
        NetCafe cafe = new NetCafe();
        ComputerSection section = new ComputerSection("CS01", "Demo", 100000m);
        Computer computer = new Computer("PC01", section, ComputerStatus.AVAILABLE);
        Employee employee = new Employee("E01", "Cashier", EmployeeStatus.ACTIVE);
        Food food = new Food("F01", "Combo", 50000m);
        cafe.AddSection(section);
        cafe.AddComputer(computer);
        cafe.AddEmployee(employee);
        cafe.AddFood(food);
        Customer[] customers = {
            new NormalCustomer("C01", "Normal customer"),
            new MemberCustomer("C02", "Member customer"),
            new VipCustomer("C03", "VIP customer")
        };
        for (int i = 0; i < customers.Length; i++)
        {
            string suffix = (i + 1).ToString();
            Customer customer = customers[i];
            cafe.AddCustomer(customer);
            CustomerAccount account = new CustomerAccount("A0" + suffix, "demo" + suffix,
                    "demo-hash-not-for-login", 0m, AccountStatus.ACTIVE, customer);
            cafe.AddAccount(account);
            DateTime start = new DateTime(2026, 10, 5, 8 + i, 0, 0);
            Session session = cafe.OpenSession("S0" + suffix, account, computer, start);
            cafe.FoodOrderService.Order("SV0" + suffix, session, employee,
                    start.AddMinutes(10), new Food[] { food }, new int[] { 1 });
            cafe.EndSession(session, start.AddHours(1));
            Receipt receipt = cafe.ReceiptService.IssueReceipt("R0" + suffix, session, employee);
            cafe.PaymentService.Pay("P0" + suffix, receipt, start.AddHours(1),
                    PaymentMethod.CASH, receipt.TotalAmount);
        }
        return cafe;
    }

    public static void Run()
    {
        NetCafe cafe = Sample();
        foreach (Receipt receipt in cafe.Receipts)
        {
            Console.WriteLine(receipt.CustomerTier
                    + " grossComputer=" + receipt.GrossComputerAmount
                    + " grossFood=" + receipt.GrossServiceAmount
                    + " computerDiscount=" + receipt.ComputerDiscountAmount
                    + " foodDiscount=" + receipt.ServiceDiscountAmount
                    + " total=" + receipt.TotalAmount);
        }
        Console.WriteLine("Collected=" + cafe.CollectedAmount());
    }
}
