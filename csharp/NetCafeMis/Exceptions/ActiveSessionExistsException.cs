namespace NetCafeMis.Exceptions;

public sealed class ActiveSessionExistsException : Exception
{
    public ActiveSessionExistsException(string message) : base(message) { }
}
