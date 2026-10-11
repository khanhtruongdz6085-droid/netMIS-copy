namespace NetCafeMis.Exceptions;

public sealed class InvalidOrderException : Exception
{
    public InvalidOrderException(string message) : base(message) { }
}
