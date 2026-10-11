namespace NetCafeMis.Exceptions;

public sealed class InactiveEmployeeException : Exception
{
    public InactiveEmployeeException(string message) : base(message) { }
}
