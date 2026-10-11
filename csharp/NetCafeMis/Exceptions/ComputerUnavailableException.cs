namespace NetCafeMis.Exceptions;

public sealed class ComputerUnavailableException : Exception
{
    public ComputerUnavailableException(string message) : base(message) { }
}
