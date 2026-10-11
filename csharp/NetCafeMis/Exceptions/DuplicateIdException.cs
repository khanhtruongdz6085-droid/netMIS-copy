namespace NetCafeMis.Exceptions;

public sealed class DuplicateIdException : Exception
{
    public DuplicateIdException(string message) : base(message) { }
}
