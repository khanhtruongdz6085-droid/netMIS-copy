namespace NetCafeMis.Exceptions;

public sealed class SessionStateException : Exception
{
    public SessionStateException(string message) : base(message) { }
}
