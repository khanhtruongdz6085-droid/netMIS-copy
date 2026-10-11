namespace NetCafeMis.Models;

/// <summary>Login data and balance, separate from the owner's discount tier.</summary>
public sealed class CustomerAccount : IIdentifiable
{
    public string Id { get; }
    public string AccountId => Id;
    public string Username { get; }
    public string PasswordHash { get; private set; }
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }
    public Customer Customer { get; }

    // The caller supplies an existing hash. This model does not authenticate.
    public CustomerAccount(string? id, string? username, string? passwordHash,
        decimal? balance, AccountStatus? status, Customer? customer)
    {
        Id = Checks.Id(id);
        Username = Checks.Name(username);
        PasswordHash = Checks.Required(passwordHash, "passwordHash");
        Checks.Name(passwordHash);
        Balance = Checks.Money(balance);
        Status = Checks.EnumValue(status, "status");
        Customer = Checks.Required(customer, "customer");
    }

    public void SetPasswordHash(string? passwordHash)
    {
        Checks.Name(passwordHash);
        PasswordHash = Checks.Required(passwordHash, "passwordHash");
    }

    public void SetBalance(decimal? balance) => Balance = Checks.Money(balance);
    public void SetStatus(AccountStatus? status) => Status = Checks.EnumValue(status, "status");
}
