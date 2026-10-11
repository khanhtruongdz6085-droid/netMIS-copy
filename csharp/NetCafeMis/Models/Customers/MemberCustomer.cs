namespace NetCafeMis.Models.Customers;

public sealed class MemberCustomer : Customer
{
    public MemberCustomer(string? id, string? name) : base(id, name) { }
    public override string TierName => "MEMBER";
    public override decimal? ComputerDiscountRate => 0.10m;
    public override decimal? FoodDiscountRate => 0.05m;
}
