using System.Collections.ObjectModel;

namespace NetCafeMis.Models;

public sealed class Session : IIdentifiable, IChargeable
{
    private readonly List<FoodService> foodServices = new();

    public string Id { get; }
    public string SessionId => Id;
    public CustomerAccount Account { get; }
    public Computer Computer { get; }
    public DateTime TimeStart { get; }
    public DateTime? TimeEnd { get; private set; }
    public SessionStatus SessionStatus { get; private set; } = SessionStatus.ACTIVE;
    public ReadOnlyCollection<FoodService> FoodServices { get; }

    public Session(string? id, CustomerAccount? account, Computer? computer, DateTime? start)
    {
        Id = Checks.Id(id);
        Account = Checks.Required(account, "account");
        Computer = Checks.Required(computer, "computer");
        TimeStart = Checks.RequiredValue(start, "start");
        FoodServices = foodServices.AsReadOnly();
    }

    /// <summary>Validate and lock an order before linking it to this session.</summary>
    public void RecordFoodService(FoodService? service)
    {
        Checks.Required(service, "foodService");
        if (SessionStatus != SessionStatus.ACTIVE)
            throw new SessionStateException("Order requires active session: " + Id);
        if (service!.Session != this)
            throw new InvalidOrderException("Order belongs to another session");
        if (service.TimeStartService < TimeStart)
            throw new InvalidTimeException("Order time precedes session start");
        if (foodServices.Any(old => old.Id == service.Id))
            throw new DuplicateIdException("Duplicate order ID: " + service.Id);
        service.Seal();
        foodServices.Add(service);
    }

    /// <summary>Direct calls and SessionService share all completion rules.</summary>
    public void EndSession(DateTime? end)
    {
        if (SessionStatus != SessionStatus.ACTIVE)
            throw new SessionStateException("Session is already completed: " + Id);
        if (end is null || end <= TimeStart)
            throw new InvalidTimeException("End must be after start: " + Id);
        if (foodServices.Any(service => service.TimeStartService > end))
            throw new InvalidTimeException("Order occurs after proposed session end");
        TimeEnd = end;
        SessionStatus = SessionStatus.COMPLETED;
        Computer.SetComputerStatus(ComputerStatus.AVAILABLE);
    }

    public decimal AmountAt(DateTime? at)
    {
        if (at is null || at < TimeStart)
            throw new InvalidTimeException("Time before session start or missing: " + Id);
        // Java Duration.getSeconds uses whole seconds; preserve that billing rule.
        decimal seconds = (at.Value.Ticks - TimeStart.Ticks) / TimeSpan.TicksPerSecond;
        return Math.Round(Computer.Section.SectionPrice * seconds / 3600m,
            0, MidpointRounding.AwayFromZero);
    }

    public decimal CalculateAmount()
    {
        if (TimeEnd is null)
            throw new SessionStateException("Session is still active: " + Id);
        return AmountAt(TimeEnd);
    }
}
