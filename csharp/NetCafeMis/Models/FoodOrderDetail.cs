namespace NetCafeMis.Models;

public sealed class FoodOrderDetail : IChargeable
{
    public FoodService Service { get; }
    public Food Food { get; }
    public int OrderQuantity { get; private set; }
    public decimal PriceAtOrder { get; }

    internal FoodOrderDetail(FoodService? service, Food? food, int quantity, decimal? price)
    {
        Service = Checks.Required(service, "service");
        Food = Checks.Required(food, "food");
        if (quantity <= 0) throw new InvalidOrderException("Quantity must be positive");
        OrderQuantity = quantity;
        PriceAtOrder = Checks.Money(price);
    }

    internal void AddQuantity(int quantity)
    {
        if (quantity <= 0 || OrderQuantity > int.MaxValue - quantity)
            throw new InvalidOrderException("Quantity must be positive and within integer range");
        OrderQuantity += quantity;
    }

    public decimal CalculateAmount() => PriceAtOrder * OrderQuantity;
}
