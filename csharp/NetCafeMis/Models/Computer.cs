namespace NetCafeMis.Models;

public sealed class Computer : IIdentifiable
{
    public string Id { get; }
    public string ComputerId => Id;
    public ComputerSection Section { get; }
    public ComputerStatus ComputerStatus { get; private set; }

    public Computer(string? id, ComputerSection? section, ComputerStatus? status)
    {
        Id = Checks.Id(id);
        Section = Checks.Required(section, "section");
        ComputerStatus = Checks.EnumValue(status, "status");
    }

    public void SetComputerStatus(ComputerStatus? status) =>
        ComputerStatus = Checks.EnumValue(status, "status");
}
