namespace NetCafeMis.Models;

public abstract class Customer : IIdentifiable, IDiscount
{
    public string Id { get; }
    public string CustomerId => Id;
    public string Name { get; }
    public abstract string TierName { get; }
    public abstract decimal? ComputerDiscountRate { get; }
    public abstract decimal? FoodDiscountRate { get; }

    protected Customer(string? id, string? name)
    {
        Id = Checks.Id(id);
        Name = Checks.Name(name);
    }
}
