namespace NetCafeMis.Models;

public sealed class Food : IIdentifiable
{
    public string Id { get; }
    public string FoodId => Id;
    public string FoodName { get; }
    public decimal Price { get; private set; }

    public Food(string? id, string? name, decimal? price)
    {
        Id = Checks.Id(id);
        FoodName = Checks.Name(name);
        Price = Checks.Money(price);
    }

    public void SetPrice(decimal? price) => Price = Checks.Money(price);
}
