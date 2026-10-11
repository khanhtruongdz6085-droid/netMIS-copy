using static NetCafeMis.Services.ServiceChecks;

namespace NetCafeMis.Services;

public sealed class SessionService
{
    private readonly NetCafeData data;
    internal SessionService(NetCafeData data) => this.data = data;

    public Session OpenSession(string? id, CustomerAccount? account, Computer? computer, DateTime? start)
    {
        Unique(data.Sessions, id);
        CustomerAccount registeredAccount = Registered(data.Accounts, account);
        if (registeredAccount.Status != AccountStatus.ACTIVE)
            throw new InactiveAccountException("Inactive account: " + registeredAccount.Id);
        Computer registeredComputer = Registered(data.Computers, computer);
        if (start is null) throw new InvalidTimeException("Session start is required");
        if (registeredComputer.ComputerStatus != ComputerStatus.AVAILABLE)
            throw new ComputerUnavailableException("Computer unavailable: " + registeredComputer.Id);
        foreach (Session old in data.Sessions)
        {
            if (old.SessionStatus == SessionStatus.ACTIVE && old.Account == registeredAccount)
                throw new ActiveSessionExistsException("Account already active: " + registeredAccount.Id);
            if ((old.Computer == registeredComputer || old.Account == registeredAccount)
                && (old.TimeEnd is null || old.TimeEnd > start))
                throw new InvalidTimeException("Session overlaps history or starts out of order");
        }
        Session session = new(id, registeredAccount, registeredComputer, start);
        data.Sessions.Add(session);
        registeredComputer.SetComputerStatus(ComputerStatus.IN_USE);
        return session;
    }

    public void EndSession(Session? session, DateTime? end) =>
        Registered(data.Sessions, session).EndSession(end);
}
