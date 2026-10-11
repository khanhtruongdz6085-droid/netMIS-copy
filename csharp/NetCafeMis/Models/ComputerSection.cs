namespace NetCafeMis.Models;

public sealed class ComputerSection : IIdentifiable
{
    public string Id { get; }
    public string SectionId => Id;
    public string SectionName { get; }
    public decimal SectionPrice { get; }

    public ComputerSection(string? id, string? name, decimal? price)
    {
        Id = Checks.Id(id);
        SectionName = Checks.Name(name);
        SectionPrice = Checks.Money(price);
    }
}
