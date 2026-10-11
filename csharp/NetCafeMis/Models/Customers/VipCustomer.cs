namespace NetCafeMis.Models.Customers;

public sealed class VipCustomer : Customer
{
    public VipCustomer(string? id, string? name) : base(id, name) { }
    public override string TierName => "VIP";
    public override decimal? ComputerDiscountRate => 0.15m;
    public override decimal? FoodDiscountRate => 0.08m;
}
