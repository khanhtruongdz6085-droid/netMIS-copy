using System.Collections.ObjectModel;

namespace NetCafeMis.Models;

public sealed class FoodService : IIdentifiable, IChargeable
{
    private readonly List<FoodOrderDetail> details = new();
    private bool sealedOrder;

    public string Id { get; }
    public string ServiceId => Id;
    public Session Session { get; }
    public Employee Employee { get; }
    public DateTime TimeStartService { get; }
    public ReadOnlyCollection<FoodOrderDetail> Details { get; }
    public decimal Total => CalculateAmount();

    public FoodService(string? id, Session? session, Employee? employee, DateTime? time)
    {
        Id = Checks.Id(id);
        Session = Checks.Required(session, "session");
        Employee = Checks.Required(employee, "employee");
        TimeStartService = Checks.RequiredValue(time, "time");
        Details = details.AsReadOnly();
    }

    public void AddItem(Food? food, int quantity, decimal? priceAtOrder)
    {
        if (sealedOrder)
            throw new InvalidOrderException("Order is recorded and cannot be changed: " + Id);
        Food item = Checks.Required(food, "food");
        decimal price = Checks.Money(priceAtOrder);
        if (quantity <= 0) throw new InvalidOrderException("Quantity must be positive");
        foreach (FoodOrderDetail detail in details)
        {
            if (detail.Food != item) continue;
            if (detail.PriceAtOrder != price)
                throw new InvalidOrderException("Different price for same food in one order");
            detail.AddQuantity(quantity);
            return;
        }
        details.Add(new FoodOrderDetail(this, item, quantity, price));
    }

    public void Seal()
    {
        if (details.Count == 0) throw new InvalidOrderException("Order must have at least one line");
        sealedOrder = true;
    }

    public decimal CalculateAmount() => details.Sum(detail => detail.CalculateAmount());
}
