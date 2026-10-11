using static NetCafeMis.Services.ServiceChecks;

namespace NetCafeMis.Services;

public sealed class PaymentService
{
    private readonly NetCafeData data;
    internal PaymentService(NetCafeData data) => this.data = data;

    public Payment Pay(string? id, Receipt? receipt, DateTime? time, PaymentMethod? method, decimal? amount)
    {
        Unique(data.Payments, id);
        Receipt registeredReceipt = Registered(data.Receipts, receipt);
        if (data.Payments.Any(payment => payment.Receipt == registeredReceipt))
            throw new PaymentAlreadyExistsException("Receipt already paid: " + registeredReceipt.Id);
        Payment payment = new(id, registeredReceipt, time, method, amount);
        data.Payments.Add(payment);
        return payment;
    }

    public decimal CollectedAmount() => data.Payments.Sum(payment => payment.Amount);
}
