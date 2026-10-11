namespace NetCafeMis.Models;

public sealed class Payment : IIdentifiable
{
    public string Id { get; }
    public string PaymentId => Id;
    public Receipt Receipt { get; }
    public DateTime Time { get; }
    public PaymentMethod PaymentMethod { get; }
    public decimal Amount { get; }

    public Payment(string? id, Receipt? receipt, DateTime? time,
        PaymentMethod? method, decimal? amount)
    {
        Id = Checks.Id(id);
        Receipt = Checks.Required(receipt, "receipt");
        Time = Checks.RequiredValue(time, "time");
        PaymentMethod = Checks.EnumValue(method, "method");
        Amount = Checks.Money(amount);
        if (Receipt.Session.TimeEnd is null)
            throw new SessionStateException("Cannot pay an active session");
        if (Time < Receipt.Session.TimeEnd)
            throw new InvalidTimeException("Payment precedes session end");
        if (Amount != Receipt.TotalAmount)
            throw new PaymentAmountMismatchException("Expected " + Receipt.TotalAmount + ", received " + amount);
    }
}
