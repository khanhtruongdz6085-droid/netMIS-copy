namespace NetCafeMis.Exceptions;

public sealed class InactiveAccountException : Exception
{
    public InactiveAccountException(string message) : base(message) { }
}
