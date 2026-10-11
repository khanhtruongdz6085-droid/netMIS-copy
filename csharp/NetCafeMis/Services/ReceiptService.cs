using static NetCafeMis.Services.ServiceChecks;

namespace NetCafeMis.Services;

public sealed class ReceiptService
{
    private readonly NetCafeData data;
    internal ReceiptService(NetCafeData data) => this.data = data;

    public Receipt IssueReceipt(string? id, Session? session, Employee? employee)
    {
        Unique(data.Receipts, id);
        Session registeredSession = Registered(data.Sessions, session);
        Employee registeredEmployee = Registered(data.Employees, employee);
        if (registeredSession.SessionStatus != SessionStatus.COMPLETED)
            throw new SessionStateException("Receipt requires completed session");
        ActiveEmployee(registeredEmployee);
        if (data.Receipts.Any(receipt => receipt.Session == registeredSession))
            throw new ReceiptAlreadyExistsException("Session already has a receipt");
        decimal serviceAmount = data.Services.Where(service => service.Session == registeredSession)
            .Sum(service => service.Total);
        Receipt receipt = new(id, registeredSession, registeredEmployee, serviceAmount,
            registeredSession.CalculateAmount(), registeredSession.Account.Customer);
        data.Receipts.Add(receipt);
        return receipt;
    }
}
