using System.Collections.ObjectModel;

namespace NetCafeMis.Services;

public sealed class NetCafe
{
    public CatalogService CatalogService { get; }
    public SessionService SessionService { get; }
    public FoodOrderService FoodOrderService { get; }
    public ReceiptService ReceiptService { get; }
    public PaymentService PaymentService { get; }
    public ReadOnlyCollection<ComputerSection> Sections { get; }
    public ReadOnlyCollection<Computer> Computers { get; }
    public ReadOnlyCollection<Customer> Customers { get; }
    public ReadOnlyCollection<CustomerAccount> Accounts { get; }
    public ReadOnlyCollection<Employee> Employees { get; }
    public ReadOnlyCollection<Food> Foods { get; }
    public ReadOnlyCollection<Session> Sessions { get; }
    public ReadOnlyCollection<FoodService> Services { get; }
    public ReadOnlyCollection<Receipt> Receipts { get; }
    public ReadOnlyCollection<Payment> Payments { get; }

    public NetCafe()
    {
        NetCafeData data = new();
        CatalogService = new CatalogService(data);
        SessionService = new SessionService(data);
        FoodOrderService = new FoodOrderService(data);
        ReceiptService = new ReceiptService(data);
        PaymentService = new PaymentService(data);
        Sections = data.Sections.AsReadOnly();
        Computers = data.Computers.AsReadOnly();
        Customers = data.Customers.AsReadOnly();
        Accounts = data.Accounts.AsReadOnly();
        Employees = data.Employees.AsReadOnly();
        Foods = data.Foods.AsReadOnly();
        Sessions = data.Sessions.AsReadOnly();
        Services = data.Services.AsReadOnly();
        Receipts = data.Receipts.AsReadOnly();
        Payments = data.Payments.AsReadOnly();
    }

    public void AddSection(ComputerSection? value) => CatalogService.AddSection(value);
    public void AddComputer(Computer? value) => CatalogService.AddComputer(value);
    public void AddCustomer(Customer? value) => CatalogService.AddCustomer(value);
    public void AddAccount(CustomerAccount? value) => CatalogService.AddAccount(value);
    public void AddEmployee(Employee? value) => CatalogService.AddEmployee(value);
    public void AddFood(Food? value) => CatalogService.AddFood(value);

    public Session OpenSession(string? id, CustomerAccount? account, Computer? computer, DateTime? start) =>
        SessionService.OpenSession(id, account, computer, start);
    public void EndSession(Session? session, DateTime? end) => SessionService.EndSession(session, end);
    public FoodService Order(string? id, Session? session, Employee? employee, DateTime? time,
        Food[]? items, int[]? quantities) => FoodOrderService.Order(id, session, employee, time, items, quantities);
    public FoodService Order(string? id, Session? session, Employee? employee, DateTime? time,
        Food[]? items, int[]? quantities, decimal?[]? prices) => FoodOrderService.Order(id, session, employee, time, items, quantities, prices);
    public Receipt IssueReceipt(string? id, Session? session, Employee? employee) => ReceiptService.IssueReceipt(id, session, employee);
    public Payment Pay(string? id, Receipt? receipt, DateTime? time, PaymentMethod? method, decimal? amount) =>
        PaymentService.Pay(id, receipt, time, method, amount);
    public decimal CollectedAmount() => PaymentService.CollectedAmount();
}
