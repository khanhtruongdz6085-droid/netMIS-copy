namespace NetCafeMis.Models;

/// <summary>Amounts, discount rates and tier are frozen when the receipt is issued.</summary>
public sealed class Receipt : IIdentifiable, IChargeable
{
    public string Id { get; }
    public string ReceiptId => Id;
    public Session Session { get; }
    public Employee Employee { get; }
    public string CustomerTier { get; }
    public decimal GrossServiceAmount { get; }
    public decimal GrossComputerAmount { get; }
    public decimal ServiceDiscountRate { get; }
    public decimal ComputerDiscountRate { get; }
    public decimal ServiceDiscountAmount { get; }
    public decimal ComputerDiscountAmount { get; }
    public decimal TotalDiscountAmount => ServiceDiscountAmount + ComputerDiscountAmount;
    public decimal ServiceAmount { get; }
    public decimal ComputerAmount { get; }
    public decimal TotalAmount => CalculateAmount();

    public Receipt(string? id, Session? session, Employee? employee,
        decimal? serviceAmount, decimal? computerAmount)
        : this(id, session, employee, serviceAmount, computerAmount,
            Checks.Required(session, "session").Account.Customer)
    { }

    public Receipt(string? id, Session? session, Employee? employee,
        decimal? grossServiceAmount, decimal? grossComputerAmount, IDiscount? policy)
    {
        Id = Checks.Id(id);
        Session = Checks.Required(session, "session");
        Employee = Checks.Required(employee, "employee");
        IDiscount discountPolicy = Checks.Required(policy, "discountPolicy");
        CustomerTier = Checks.Name(discountPolicy.TierName);
        ComputerDiscountRate = Rate(discountPolicy.ComputerDiscountRate);
        ServiceDiscountRate = Rate(discountPolicy.FoodDiscountRate);
        GrossServiceAmount = Checks.Money(grossServiceAmount);
        GrossComputerAmount = Checks.Money(grossComputerAmount);
        ServiceDiscountAmount = Discount(GrossServiceAmount, ServiceDiscountRate);
        ComputerDiscountAmount = Discount(GrossComputerAmount, ComputerDiscountRate);
        ServiceAmount = GrossServiceAmount - ServiceDiscountAmount;
        ComputerAmount = GrossComputerAmount - ComputerDiscountAmount;
    }

    private static decimal Rate(decimal? value)
    {
        if (value is null || value < 0 || value > 1)
            throw new ValidationException("Discount rate must be between 0 and 1");
        return value.Value;
    }

    private static decimal Discount(decimal gross, decimal rate) =>
        Math.Min(Math.Round(gross * rate, 0, MidpointRounding.AwayFromZero), gross);

    public decimal CalculateAmount() => ServiceAmount + ComputerAmount;
}
