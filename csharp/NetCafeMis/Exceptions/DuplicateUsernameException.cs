namespace NetCafeMis.Exceptions;

public sealed class DuplicateUsernameException : Exception
{
    public DuplicateUsernameException(string message) : base(message) { }
}
