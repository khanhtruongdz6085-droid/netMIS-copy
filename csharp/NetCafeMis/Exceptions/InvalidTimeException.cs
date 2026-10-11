namespace NetCafeMis.Exceptions;

public sealed class InvalidTimeException : Exception
{
    public InvalidTimeException(string message) : base(message) { }
}
