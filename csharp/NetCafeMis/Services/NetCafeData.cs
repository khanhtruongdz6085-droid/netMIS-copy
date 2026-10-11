namespace NetCafeMis.Services;

internal sealed class NetCafeData
{
    internal readonly List<ComputerSection> Sections = new();
    internal readonly List<Computer> Computers = new();
    internal readonly List<Customer> Customers = new();
    internal readonly List<CustomerAccount> Accounts = new();
    internal readonly List<Employee> Employees = new();
    internal readonly List<Food> Foods = new();
    internal readonly List<Session> Sessions = new();
    internal readonly List<FoodService> Services = new();
    internal readonly List<Receipt> Receipts = new();
    internal readonly List<Payment> Payments = new();
}
