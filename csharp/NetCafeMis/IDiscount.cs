namespace NetCafeMis;

public interface IDiscount
{
    string TierName { get; }
    decimal? ComputerDiscountRate { get; }
    decimal? FoodDiscountRate { get; }
}
