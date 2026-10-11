namespace NetCafeMis.Exceptions;

public sealed class ReceiptAlreadyExistsException : Exception
{
    public ReceiptAlreadyExistsException(string message) : base(message) { }
}
