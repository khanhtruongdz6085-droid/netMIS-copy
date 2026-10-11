namespace NetCafeMis.Models.Customers;

public sealed class NormalCustomer : Customer
{
    public NormalCustomer(string? id, string? name) : base(id, name) { }
    public override string TierName => "NORMAL";
    public override decimal? ComputerDiscountRate => 0m;
    public override decimal? FoodDiscountRate => 0m;
}
