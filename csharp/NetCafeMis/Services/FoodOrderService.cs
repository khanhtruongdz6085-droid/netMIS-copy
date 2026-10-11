using static NetCafeMis.Services.ServiceChecks;

namespace NetCafeMis.Services;

public sealed class FoodOrderService
{
    private readonly NetCafeData data;
    internal FoodOrderService(NetCafeData data) => this.data = data;

    public FoodService Order(string? id, Session? session, Employee? employee, DateTime? time,
        Food[]? items, int[]? quantities)
    {
        if (items is null) throw new InvalidOrderException("Order items are required");
        decimal?[] prices = new decimal?[items.Length];
        for (int i = 0; i < items.Length; i++) prices[i] = Registered(data.Foods, items[i]).Price;
        return Order(id, session, employee, time, items, quantities, prices);
    }

    public FoodService Order(string? id, Session? session, Employee? employee, DateTime? time,
        Food[]? items, int[]? quantities, decimal?[]? prices)
    {
        Unique(data.Services, id);
        Session registeredSession = Registered(data.Sessions, session);
        Employee registeredEmployee = Registered(data.Employees, employee);
        if (registeredSession.SessionStatus != SessionStatus.ACTIVE)
            throw new SessionStateException("Order requires active session");
        ActiveEmployee(registeredEmployee);
        if (time is null || time < registeredSession.TimeStart)
            throw new InvalidTimeException("Order time precedes session start or is missing");
        if (items is null || quantities is null || prices is null || items.Length == 0
            || items.Length != quantities.Length || items.Length != prices.Length)
            throw new InvalidOrderException("Order must contain matching nonempty lines");
        FoodService service = new(id, registeredSession, registeredEmployee, time);
        for (int i = 0; i < items.Length; i++)
            service.AddItem(Registered(data.Foods, items[i]), quantities[i], prices[i]);
        registeredSession.RecordFoodService(service);
        data.Services.Add(service);
        return service;
    }
}
