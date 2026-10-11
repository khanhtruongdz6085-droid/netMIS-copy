using static NetCafeMis.Services.ServiceChecks;

namespace NetCafeMis.Services;

public sealed class CatalogService
{
    private readonly NetCafeData data;
    internal CatalogService(NetCafeData data) => this.data = data;

    public void AddSection(ComputerSection? value) => Add(data.Sections, value);
    public void AddCustomer(Customer? value) => Add(data.Customers, value);
    public void AddEmployee(Employee? value) => Add(data.Employees, value);
    public void AddFood(Food? value) => Add(data.Foods, value);

    public void AddAccount(CustomerAccount? value)
    {
        CustomerAccount account = Required(value, "account");
        Unique(data.Accounts, account.Id);
        Registered(data.Customers, account.Customer);
        if (data.Accounts.Any(old => string.Equals(old.Username, account.Username, StringComparison.OrdinalIgnoreCase)))
            throw new DuplicateUsernameException("Duplicate username: " + account.Username);
        data.Accounts.Add(account);
    }

    public void AddComputer(Computer? value)
    {
        Computer computer = Required(value, "computer");
        Registered(data.Sections, computer.Section);
        if (computer.ComputerStatus == ComputerStatus.IN_USE)
            throw new ComputerUnavailableException("New computer cannot be IN_USE without a session");
        Add(data.Computers, computer);
    }
}
