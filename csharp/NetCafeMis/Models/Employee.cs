namespace NetCafeMis.Models;

public sealed class Employee : IIdentifiable
{
    public string Id { get; }
    public string EmployeeId => Id;
    public string EmployeeName { get; }
    public EmployeeStatus EmployeeStatus { get; private set; }

    public Employee(string? id, string? name, EmployeeStatus? status)
    {
        Id = Checks.Id(id);
        EmployeeName = Checks.Name(name);
        EmployeeStatus = Checks.EnumValue(status, "status");
    }

    public void SetEmployeeStatus(EmployeeStatus? status) =>
        EmployeeStatus = Checks.EnumValue(status, "status");
}
